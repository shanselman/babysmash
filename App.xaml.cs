using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Updatum;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using WinForms = System.Windows.Forms;

namespace BabySmash
{
    public partial class App : Application
    {
        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static Mutex _singleInstanceMutex;

        internal static readonly UpdatumManager AppUpdater = new("shanselman", "babysmash")
        {
            FetchOnlyLatestRelease = true,
            InstallUpdateSingleFileExecutableName = "BabySmash",
        };

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Check for updates BEFORE launching the game
            // This way the parent can handle updates before baby takes over
            var shouldLaunch = await CheckForUpdatesBeforeLaunchAsync();
            
            if (shouldLaunch)
            {
                Controller.Instance.Launch();
            }
        }

        private async Task<bool> CheckForUpdatesBeforeLaunchAsync()
        {
            try
            {
                var updateFound = await AppUpdater.CheckForUpdatesAsync();
                if (!updateFound) return true;

                var release = AppUpdater.LatestRelease!;
                var changelog = AppUpdater.GetChangelog(true) ?? "No release notes available.";

                var dialog = new UpdateDialog(release.TagName, changelog);
                dialog.ShowDialog();

                if (dialog.Result == UpdateDialog.UpdateDialogResult.Download)
                {
                    var installed = await DownloadAndInstallUpdateAsync();
                    // If install succeeded, app will restart - don't launch
                    // If install failed, let user continue to the app
                    return !installed;
                }

                // RemindLater or Skip - continue to launch
                return true;
            }
            catch (Exception ex)
            {
                // Update check failed - don't block the app, just launch
                Debug.WriteLine($"Update check failed: {ex.Message}");
                return true;
            }
        }

        private async Task<bool> DownloadAndInstallUpdateAsync()
        {
            DownloadProgressDialog progressDialog = null;
            try
            {
                progressDialog = new DownloadProgressDialog(AppUpdater);
                progressDialog.Show();

                var downloadedAsset = await AppUpdater.DownloadUpdateAsync();

                progressDialog?.Close();
                progressDialog = null;

                if (downloadedAsset == null)
                {
                    MessageBox.Show("Failed to download the update. Please try again later.",
                        "Download Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (!System.IO.File.Exists(downloadedAsset.FilePath))
                {
                    MessageBox.Show($"Update file was deleted or is inaccessible:\n{downloadedAsset.FilePath}\n\nThis may be caused by antivirus software.",
                        "Update File Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                var confirmResult = MessageBox.Show(
                    "The update has been downloaded. BabySmash! will now restart to install the update.\n\nContinue?",
                    "Install Update",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    await AppUpdater.InstallUpdateAsync(downloadedAsset);
                    return true; // App will restart
                }

                return false; // User cancelled, continue to app
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access denied when accessing update file.\n\n1. Antivirus may be blocking the update\n2. Windows SmartScreen may need approval\n\nError: {ex.Message}",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download or install update: {ex.Message}",
                    "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                progressDialog?.Close();
            }
        }

        public App()
        {
            // Prevent multiple instances from running at the same time
            bool createdNew;
            _singleInstanceMutex = new Mutex(true, "BabySmashSingleInstance", out createdNew);
            if (!createdNew)
            {
                Shutdown();
                return;
            }

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
            try
            {
                _hookID = InterceptKeys.SetHook(_proc);
            }
            catch
            {
                DetachKeyboardHook();
            }
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            DetachKeyboardHook();
            _singleInstanceMutex?.Dispose();
        }

        /// <summary>
        /// Detach the keyboard hook; call during shutdown to prevent calls as we unload
        /// </summary>
        private static void DetachKeyboardHook()
        {
            if (_hookID != IntPtr.Zero)
                InterceptKeys.UnhookWindowsHookEx(_hookID);
        }

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool alt = (WinForms.Control.ModifierKeys & Keys.Alt) != 0;
                bool control = (WinForms.Control.ModifierKeys & Keys.Control) != 0;

                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (alt && key == Keys.F4)
                {
                    Application.Current.Shutdown();
                    return (IntPtr)1; // Handled.
                }

                if (!AllowKeyboardInput(alt, control, key))
                {
                    return (IntPtr)1; // Handled.
                }
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>Determines whether the specified keyboard input should be allowed to be processed by the system.</summary>
        /// <remarks>Helps block unwanted keys and key combinations that could exit the app, make system changes, etc.</remarks>
        public static bool AllowKeyboardInput(bool alt, bool control, Keys key)
        {
            // Disallow various special keys.
            // F4 alone triggers Properties command in Windows - kids hit it accidentally
            if (key <= Keys.Back || key == Keys.None ||
                key == Keys.Menu || key == Keys.Pause ||
                key == Keys.Help || key == Keys.F4)
            {
                return false;
            }

            // Disallow ranges of special keys.
            // Currently leaves volume controls enabled; consider if this makes sense.
            // Disables non-existing Keys up to 65534, to err on the side of caution for future keyboard expansion.
            if ((key >= Keys.LWin && key <= Keys.Sleep) ||
                (key >= Keys.KanaMode && key <= Keys.HanjaMode) ||
                (key >= Keys.IMEConvert && key <= Keys.IMEModeChange) ||
                (key >= Keys.BrowserBack && key <= Keys.BrowserHome) ||
                (key >= Keys.MediaNextTrack && key <= Keys.LaunchApplication2) ||
                (key >= Keys.ProcessKey && key <= (Keys)65534))
            {
                return false;
            }

            // Disallow specific key combinations. (These component keys would be OK on their own.)
            if ((alt && key == Keys.Tab) ||
                (alt && key == Keys.Space) ||
                (control && key == Keys.Escape))
            {
                return false;
            }

            // Allow anything else (like letters, numbers, spacebar, braces, and so on).
            return true;
        }
    }
}