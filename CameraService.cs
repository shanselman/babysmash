using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BabySmash.Properties;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
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
    /// Keeps a recent webcam snapshot around for "silly selfie" shapes.
    /// Every camera failure is caught and reported through <see cref="Status"/>;
    /// nothing in here should ever take the game down.
    /// </summary>
    public sealed class CameraService : IDisposable
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(1.5);
        private const int MaxConsecutiveFailures = 3;
        private const int SnapshotWidth = 480;

        private readonly SemaphoreSlim applyLock = new SemaphoreSlim(1, 1);
        private MediaCapture mediaCapture;
        private LowLagPhotoCapture photoCapture;
        private CancellationTokenSource refreshCts;
        private Task refreshTask;
        private string activeRequestedDeviceId;
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
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                return devices.Select(device => new CameraInfo(device.Id, device.Name)).ToList();
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

                    if (mediaCapture != null &&
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

        private async Task StartAsync(string requestedDeviceId)
        {
            SetStatus(CameraStatus.Starting, "Camera starting...");

            IReadOnlyList<CameraInfo> cameras = await GetCamerasAsync();
            CameraInfo camera = cameras.FirstOrDefault(candidate => candidate.Id == requestedDeviceId)
                                ?? cameras.FirstOrDefault();
            if (camera == null)
            {
                SetStatus(CameraStatus.Error, "Camera error: no camera found");
                return;
            }

            var capture = new MediaCapture();
            try
            {
                capture.Failed += HandleCaptureFailed;
                await InitializeCaptureAsync(capture, camera.Id);
                photoCapture = await capture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateJpeg());
            }
            catch
            {
                capture.Failed -= HandleCaptureFailed;
                photoCapture = null;
                capture.Dispose();
                throw;
            }

            mediaCapture = capture;
            activeRequestedDeviceId = requestedDeviceId;
            SetStatus(CameraStatus.Enabled, $"Camera enabled - {camera.Name}");

            refreshCts = new CancellationTokenSource();
            CancellationToken token = refreshCts.Token;
            LowLagPhotoCapture lowLagCapture = photoCapture;
            refreshTask = Task.Run(() => RefreshLoopAsync(lowLagCapture, token));
        }

        private static async Task InitializeCaptureAsync(MediaCapture capture, string deviceId)
        {
            try
            {
                await capture.InitializeAsync(CreateSettings(deviceId, MediaCaptureSharingMode.ExclusiveControl));
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Another app may own the camera; a read-only share still lets us grab snapshots.
                Debug.WriteLine($"Exclusive camera access failed, retrying shared: {ex.Message}");
                await capture.InitializeAsync(CreateSettings(deviceId, MediaCaptureSharingMode.SharedReadOnly));
            }
        }

        private static MediaCaptureInitializationSettings CreateSettings(string deviceId, MediaCaptureSharingMode sharingMode)
        {
            return new MediaCaptureInitializationSettings
            {
                VideoDeviceId = deviceId,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                SharingMode = sharingMode
            };
        }

        private async Task RefreshLoopAsync(LowLagPhotoCapture capture, CancellationToken token)
        {
            int failures = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    CapturedPhoto photo = await capture.CaptureAsync().AsTask(token).ConfigureAwait(false);
                    using (CapturedFrame frame = photo.Frame)
                    {
                        latestFrame = DecodeFrame(frame);
                    }

                    failures = 0;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Camera snapshot failed: {ex.Message}");
                    if (++failures >= MaxConsecutiveFailures)
                    {
                        latestFrame = null;
                        SetStatus(CameraStatus.Error, DescribeError(ex));
                        break;
                    }
                }

                try
                {
                    await Task.Delay(RefreshInterval, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private static BitmapSource DecodeFrame(IRandomAccessStream frame)
        {
            using Stream source = frame.AsStreamForRead();
            using var buffer = new MemoryStream();
            source.CopyTo(buffer);
            buffer.Position = 0;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = SnapshotWidth;
            bitmap.StreamSource = buffer;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private void HandleCaptureFailed(MediaCapture sender, MediaCaptureFailedEventArgs args)
        {
            // Typically the camera was unplugged or grabbed by another app. Stop snapshotting;
            // the next ApplySettingsAsync (e.g. after closing Options) will try again.
            try
            {
                refreshCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Already stopping.
            }

            latestFrame = null;
            SetStatus(CameraStatus.Error, $"Camera error: {Shorten(args.Message)}");
        }

        private async Task StopAsync()
        {
            refreshCts?.Cancel();
            if (refreshTask != null)
            {
                try
                {
                    await refreshTask;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Camera refresh loop ended with: {ex.Message}");
                }

                refreshTask = null;
            }

            refreshCts?.Dispose();
            refreshCts = null;

            if (photoCapture != null)
            {
                try
                {
                    await photoCapture.FinishAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to finish photo capture: {ex.Message}");
                }

                photoCapture = null;
            }

            ReleaseMediaCapture();
            activeRequestedDeviceId = null;
            latestFrame = null;
        }

        private void ReleaseMediaCapture()
        {
            if (mediaCapture == null)
            {
                return;
            }

            mediaCapture.Failed -= HandleCaptureFailed;
            try
            {
                mediaCapture.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to release camera: {ex.Message}");
            }

            mediaCapture = null;
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
            if (ex is UnauthorizedAccessException)
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
            refreshCts?.Cancel();
            photoCapture = null;
            ReleaseMediaCapture();
            latestFrame = null;
        }
    }
}
