using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BabySmash.Properties;
using FlashCap;
using Application = System.Windows.Application;

namespace BabySmash
{
    public enum CameraStatus
    {
        Disabled,
        Starting,
        Enabled,
        Error
    }

    public sealed record CameraInfo(string Id, string Name)
    {
        public override string ToString() => Name;
    }

    /// <summary>
    /// Keeps a recent webcam snapshot around for "silly selfie" shapes, using FlashCap.
    /// Every camera failure is caught and reported through <see cref="Status"/>;
    /// nothing in here should ever take the game down.
    /// </summary>
    public sealed class CameraService : IDisposable
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(1.5);
        private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(6);
        private static readonly TimeSpan DisposeTimeout = TimeSpan.FromSeconds(2);
        private const int MaxConsecutiveFailures = 3;
        private const int PreferredCaptureWidth = 640;
        private const int SnapshotWidth = 480;
        private const int AccessDeniedHResult = unchecked((int)0x80070005);

        private readonly SemaphoreSlim applyLock = new SemaphoreSlim(1, 1);
        private CaptureDevice captureDevice;
        private CancellationTokenSource watchdogCts;
        private Task watchdogTask;
        private string activeRequestedDeviceId;
        private long lastFrameTicks;
        private long lastDecodeTicks;
        private int consecutiveFailures;
        private volatile BitmapSource latestFrame;
        private volatile CameraStatus status = CameraStatus.Disabled;
        private volatile string statusText = string.Empty;
        private bool disposed;

        /// <summary>Raised on the WPF dispatcher whenever <see cref="Status"/> changes.</summary>
        public event EventHandler StatusChanged;

        public CameraStatus Status => status;

        public string StatusText => statusText;

        /// <summary>The most recent snapshot, or null when the camera is not running.</summary>
        public BitmapSource LatestFrame => status == CameraStatus.Enabled ? latestFrame : null;

        public static async Task<IReadOnlyList<CameraInfo>> GetCamerasAsync()
        {
            try
            {
                List<CaptureDeviceDescriptor> descriptors = await Task.Run(FindDescriptors);
                return descriptors.Select(descriptor => new CameraInfo(GetDeviceId(descriptor), descriptor.Name)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to enumerate cameras: {ex}");
                return Array.Empty<CameraInfo>();
            }
        }

        /// <summary>
        /// Starts, stops or switches the camera to match the current settings. Never throws.
        /// </summary>
        public async Task ApplySettingsAsync()
        {
            try
            {
                await applyLock.WaitAsync();
                try
                {
                    if (disposed)
                    {
                        return;
                    }

                    string requestedDeviceId = Settings.Default.CameraDeviceId ?? string.Empty;
                    if (!Settings.Default.CameraEnabled)
                    {
                        await StopAsync();
                        SetStatus(CameraStatus.Disabled, string.Empty);
                        return;
                    }

                    if (captureDevice != null &&
                        status == CameraStatus.Enabled &&
                        requestedDeviceId == activeRequestedDeviceId)
                    {
                        return;
                    }

                    await StopAsync();
                    await StartAsync(requestedDeviceId);
                }
                finally
                {
                    applyLock.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Camera settings could not be applied: {ex}");
                latestFrame = null;
                SetStatus(CameraStatus.Error, DescribeError(ex));
            }
        }

        /// <summary>
        /// FlashCap lists each camera once per backend (DirectShow, Media Foundation, VfW, ...).
        /// Keep one entry per camera, preferring DirectShow, and skip the legacy VfW "Default" device.
        /// </summary>
        private static List<CaptureDeviceDescriptor> FindDescriptors()
        {
            return new CaptureDevices()
                .EnumerateDescriptors()
                .Where(descriptor => descriptor.DeviceType != DeviceTypes.VideoForWindows &&
                                     descriptor.Characteristics.Any(IsUsable))
                .OrderBy(descriptor => descriptor.DeviceType == DeviceTypes.DirectShow ? 0 : 1)
                .GroupBy(descriptor => descriptor.Name)
                .Select(group => group.First())
                .ToList();
        }

        private static bool IsUsable(VideoCharacteristics characteristics) =>
            characteristics.PixelFormat != PixelFormats.Unknown;

        private static string GetDeviceId(CaptureDeviceDescriptor descriptor) =>
            descriptor.Identity?.ToString() ?? descriptor.Name;

        /// <summary>Picks a modest resolution (about 640 wide), preferring JPEG since it is cheap to decode.</summary>
        private static VideoCharacteristics ChooseCharacteristics(CaptureDeviceDescriptor descriptor)
        {
            return descriptor.Characteristics
                .Where(IsUsable)
                .OrderBy(characteristics => Math.Abs(characteristics.Width - PreferredCaptureWidth))
                .ThenBy(characteristics => characteristics.PixelFormat == PixelFormats.JPEG ? 0 : 1)
                .First();
        }

        private async Task StartAsync(string requestedDeviceId)
        {
            SetStatus(CameraStatus.Starting, "Camera starting...");

            List<CaptureDeviceDescriptor> descriptors = await Task.Run(FindDescriptors);
            CaptureDeviceDescriptor descriptor = descriptors.FirstOrDefault(candidate => GetDeviceId(candidate) == requestedDeviceId)
                                                 ?? descriptors.FirstOrDefault();
            if (descriptor == null)
            {
                SetStatus(CameraStatus.Error, "Camera error: no camera found");
                return;
            }

            VideoCharacteristics characteristics = ChooseCharacteristics(descriptor);
            Interlocked.Exchange(ref lastFrameTicks, Environment.TickCount64);
            Interlocked.Exchange(ref lastDecodeTicks, 0);
            consecutiveFailures = 0;

            // Open on a worker thread so camera driver start-up never stalls the game.
            CaptureDevice device = await Task.Run(() => descriptor.OpenAsync(characteristics, OnPixelBufferArrived));
            try
            {
                await Task.Run(() => device.StartAsync());
            }
            catch
            {
                DisposeDevice(device);
                throw;
            }

            captureDevice = device;
            activeRequestedDeviceId = requestedDeviceId;
            SetStatus(CameraStatus.Enabled, $"Camera enabled - {descriptor.Name}");

            watchdogCts = new CancellationTokenSource();
            CancellationToken token = watchdogCts.Token;
            watchdogTask = Task.Run(() => WatchdogAsync(token));
        }

        /// <summary>Called by FlashCap for every frame on its own thread; only decodes one every RefreshInterval.</summary>
        private void OnPixelBufferArrived(PixelBufferScope bufferScope)
        {
            long now = Environment.TickCount64;
            Interlocked.Exchange(ref lastFrameTicks, now);
            if (now - Interlocked.Read(ref lastDecodeTicks) < RefreshInterval.TotalMilliseconds)
            {
                return;
            }

            Interlocked.Exchange(ref lastDecodeTicks, now);
            try
            {
                latestFrame = DecodeFrame(bufferScope.Buffer.ReferImage());
                consecutiveFailures = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Camera snapshot failed: {ex.Message}");
                if (++consecutiveFailures >= MaxConsecutiveFailures && status == CameraStatus.Enabled)
                {
                    latestFrame = null;
                    SetStatus(CameraStatus.Error, DescribeError(ex));
                }
            }
        }

        /// <summary>
        /// FlashCap has no "device lost" event, so treat a long gap between frames
        /// (camera unplugged, blocked by privacy settings, grabbed by another app) as an error.
        /// The next ApplySettingsAsync (e.g. after closing Options) will try again.
        /// </summary>
        private async Task WatchdogAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                long silence = Environment.TickCount64 - Interlocked.Read(ref lastFrameTicks);
                if (status == CameraStatus.Enabled && silence > FrameTimeout.TotalMilliseconds)
                {
                    latestFrame = null;
                    SetStatus(CameraStatus.Error, "Camera error: no picture from camera (unplugged or in use?)");
                    break;
                }
            }
        }

        private static BitmapSource DecodeFrame(ArraySegment<byte> image)
        {
            // FlashCap hands us a complete BMP or JPEG image; decode it fully before the buffer is reused.
            using var buffer = new MemoryStream(image.Array, image.Offset, image.Count, writable: false);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = SnapshotWidth;
            bitmap.StreamSource = buffer;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private async Task StopAsync()
        {
            watchdogCts?.Cancel();
            if (watchdogTask != null)
            {
                try
                {
                    await watchdogTask;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Camera watchdog ended with: {ex.Message}");
                }

                watchdogTask = null;
            }

            watchdogCts?.Dispose();
            watchdogCts = null;

            CaptureDevice device = captureDevice;
            captureDevice = null;
            if (device != null)
            {
                try
                {
                    await Task.Run(() => device.StopAsync());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to stop camera: {ex.Message}");
                }

                DisposeDevice(device);
            }

            activeRequestedDeviceId = null;
            latestFrame = null;
        }

        private static void DisposeDevice(CaptureDevice device)
        {
            try
            {
                // Bounded so a misbehaving driver can't hang shutdown.
                Task.Run(device.Dispose).Wait(DisposeTimeout);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to release camera: {ex.Message}");
            }
        }

        private void SetStatus(CameraStatus newStatus, string newText)
        {
            status = newStatus;
            statusText = newText;

            Dispatcher dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null ||
                dispatcher.HasShutdownStarted ||
                dispatcher.HasShutdownFinished)
            {
                return;
            }

            try
            {
                dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action(() => StatusChanged?.Invoke(this, EventArgs.Empty)));
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Unable to report camera status during shutdown: {ex.Message}");
            }
        }

        private static string DescribeError(Exception ex)
        {
            if (ex is UnauthorizedAccessException || ex.HResult == AccessDeniedHResult)
            {
                return "Camera error: access denied (check Windows camera privacy settings)";
            }

            return $"Camera error: {Shorten(ex.Message)}";
        }

        private static string Shorten(string message)
        {
            string firstLine = (message ?? "unknown problem").Trim().Split('\r', '\n')[0];
            return firstLine.Length > 80 ? firstLine.Substring(0, 77) + "..." : firstLine;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            StatusChanged = null;
            try
            {
                watchdogCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Already stopping.
            }

            CaptureDevice device = captureDevice;
            captureDevice = null;
            if (device != null)
            {
                DisposeDevice(device);
            }

            latestFrame = null;
        }
    }
}
