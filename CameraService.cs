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

    public sealed record CameraInfo(string Id, string Name);

    /// <summary>
    /// Keeps a recent webcam snapshot around for "silly selfie" shapes, using FlashCap.
    /// Every camera failure is caught and reported through <see cref="Status"/>;
    /// nothing in here should ever take the game down.
    /// </summary>
    public sealed class CameraService : IDisposable
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(1.5);
        private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(6);
        private static readonly TimeSpan WarmUpPeriod = TimeSpan.FromSeconds(3);
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
        private int generation;
        private long startedTicks;
        private long lastFrameTicks;
        private long lastDecodeTicks;
        private int consecutiveFailures;
        private volatile BitmapSource latestFrame;
        private volatile CameraStatus status = CameraStatus.Disabled;
        private volatile string statusText = string.Empty;
        private volatile bool disposed;

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
        /// Use DirectShow devices, plus any camera that only another backend can see, and skip
        /// the legacy VfW "Default" device. Two different cameras with the same name both stay listed.
        /// </summary>
        private static List<CaptureDeviceDescriptor> FindDescriptors()
        {
            List<CaptureDeviceDescriptor> usable = new CaptureDevices()
                .EnumerateDescriptors()
                .Where(descriptor => descriptor.DeviceType != DeviceTypes.VideoForWindows &&
                                     descriptor.Characteristics.Any(IsUsable))
                .ToList();

            List<CaptureDeviceDescriptor> directShow = usable
                .Where(descriptor => descriptor.DeviceType == DeviceTypes.DirectShow)
                .ToList();
            var directShowNames = new HashSet<string>(directShow.Select(descriptor => descriptor.Name));

            return directShow
                .Concat(usable.Where(descriptor => descriptor.DeviceType != DeviceTypes.DirectShow &&
                                                   !directShowNames.Contains(descriptor.Name)))
                .ToList();
        }

        private static bool IsUsable(VideoCharacteristics characteristics) =>
            characteristics.PixelFormat != PixelFormats.Unknown;

        private static string GetDeviceId(CaptureDeviceDescriptor descriptor) =>
            descriptor.Identity?.ToString() ?? descriptor.Name;

        /// <summary>
        /// Picks a modest resolution (about 640 wide). Uncompressed formats come first: FlashCap turns them
        /// into BMPs that always decode, while many webcams send MJPEG frames without Huffman tables,
        /// which WPF's JPEG decoder can reject.
        /// </summary>
        private static VideoCharacteristics ChooseCharacteristics(CaptureDeviceDescriptor descriptor)
        {
            return descriptor.Characteristics
                .Where(IsUsable)
                .OrderBy(characteristics => characteristics.PixelFormat == PixelFormats.JPEG ? 1 : 0)
                .ThenBy(characteristics => Math.Abs(characteristics.Width - PreferredCaptureWidth))
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
            int deviceGeneration = Interlocked.Increment(ref generation);
            consecutiveFailures = 0;
            Interlocked.Exchange(ref lastDecodeTicks, 0);

            // Open on a worker thread so camera driver start-up never stalls the game.
            CaptureDevice device = await Task.Run(
                () => descriptor.OpenAsync(characteristics, bufferScope => OnPixelBufferArrived(bufferScope, deviceGeneration)));
            try
            {
                if (!disposed)
                {
                    await Task.Run(() => device.StartAsync());
                }
            }
            catch
            {
                await DisposeDeviceAsync(device);
                throw;
            }

            // The app may have shut down while the driver was starting; don't leave the camera on.
            if (disposed)
            {
                await DisposeDeviceAsync(device);
                return;
            }

            captureDevice = device;
            activeRequestedDeviceId = requestedDeviceId;

            // Start the clocks only now, so slow driver start-up doesn't count as "no frames".
            long now = Environment.TickCount64;
            Interlocked.Exchange(ref startedTicks, now);
            Interlocked.Exchange(ref lastFrameTicks, now);
            SetStatus(CameraStatus.Enabled, $"Camera enabled - {descriptor.Name}");

            watchdogCts = new CancellationTokenSource();
            CancellationToken token = watchdogCts.Token;
            watchdogTask = Task.Run(() => WatchdogAsync(deviceGeneration, token));
        }

        /// <summary>Called by FlashCap for every frame on its own thread; only decodes one every RefreshInterval.</summary>
        private void OnPixelBufferArrived(PixelBufferScope bufferScope, int deviceGeneration)
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

                // Cameras often send a few bad frames while warming up; only count failures after that.
                bool warmingUp = now - Interlocked.Read(ref startedTicks) < WarmUpPeriod.TotalMilliseconds;
                if (!warmingUp && ++consecutiveFailures == MaxConsecutiveFailures)
                {
                    // Leave FlashCap's callback thread before stopping the device.
                    string message = DescribeError(ex);
                    _ = Task.Run(() => FailAsync(deviceGeneration, message));
                }
            }
        }

        /// <summary>
        /// FlashCap has no "device lost" event, so treat a long gap between frames
        /// (camera unplugged, blocked by privacy settings, grabbed by another app) as an error.
        /// </summary>
        private async Task WatchdogAsync(int deviceGeneration, CancellationToken token)
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
                if (silence > FrameTimeout.TotalMilliseconds)
                {
                    _ = Task.Run(() => FailAsync(deviceGeneration, "Camera error: no picture from camera (unplugged or in use?)"));
                    break;
                }
            }
        }

        /// <summary>
        /// Shuts the camera off (so its light goes out) and reports the error.
        /// The next ApplySettingsAsync, e.g. after closing Options, tries again.
        /// </summary>
        private async Task FailAsync(int deviceGeneration, string message)
        {
            try
            {
                await applyLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    // Ignore failures from a device that has already been replaced or stopped.
                    if (disposed || deviceGeneration != Volatile.Read(ref generation) || captureDevice == null)
                    {
                        return;
                    }

                    await StopAsync().ConfigureAwait(false);
                    SetStatus(CameraStatus.Error, message);
                }
                finally
                {
                    applyLock.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to stop failed camera: {ex}");
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
            CancellationTokenSource cts = watchdogCts;
            Task watchdog = watchdogTask;
            watchdogCts = null;
            watchdogTask = null;

            cts?.Cancel();
            if (watchdog != null)
            {
                try
                {
                    await watchdog.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Camera watchdog ended with: {ex.Message}");
                }
            }

            cts?.Dispose();

            CaptureDevice device = captureDevice;
            captureDevice = null;
            activeRequestedDeviceId = null;
            latestFrame = null;

            if (device != null)
            {
                try
                {
                    await Task.Run(() => device.StopAsync()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to stop camera: {ex.Message}");
                }

                await DisposeDeviceAsync(device).ConfigureAwait(false);
            }
        }

        /// <summary>Releases the device off the UI thread, giving up after a timeout so a bad driver can't hang us.</summary>
        private static async Task DisposeDeviceAsync(CaptureDevice device)
        {
            try
            {
                await Task.Run(device.Dispose).WaitAsync(DisposeTimeout).ConfigureAwait(false);
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

        /// <summary>
        /// Called on app exit. Blocks briefly (bounded) so the camera is released before the process ends;
        /// a start still in progress sees <see cref="disposed"/> and releases its own device.
        /// </summary>
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
            latestFrame = null;
            if (device != null)
            {
                try
                {
                    Task.Run(device.Dispose).Wait(DisposeTimeout);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to release camera on exit: {ex.Message}");
                }
            }
        }
    }
}
