using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using BabySmash.Properties;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace BabySmash
{
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            Loaded += Options_Loaded;
        }

        private async void Options_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IReadOnlyList<CameraInfo> cameras = await CameraService.GetCamerasAsync();
                var choices = new List<CameraInfo>
                {
                    new CameraInfo(string.Empty, cameras.Count == 0 ? "No camera found" : "Default camera")
                };
                choices.AddRange(cameras);

                // Keep an unplugged saved camera selectable so opening Options doesn't silently forget it.
                string savedId = Settings.Default.CameraDeviceId;
                if (!string.IsNullOrEmpty(savedId) && choices.All(choice => choice.Id != savedId))
                {
                    choices.Add(new CameraInfo(savedId, "Saved camera (not connected)"));
                }

                // The XAML SelectedValue binding picks the saved camera once items are available.
                CameraChooser.ItemsSource = choices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to list cameras for Options: {ex}");
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            Close();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Mouse.Capture(this, CaptureMode.SubTree);
            // Get version from assembly
            versionLabel.Text = "Version " + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "2.0.0");
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Mouse.Capture(null);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //Mouse.Capture(null);
            base.OnMouseMove(e);
        }

        protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // hitting "escape" is the new cancel button
                Settings.Default.Reload();
                Close();
            }
            else
            {
                base.OnKeyUp(e);
            }
        }

        private void FeedbackLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/shanselman/babysmash/issues") { UseShellExecute = true });
        }
    }
}