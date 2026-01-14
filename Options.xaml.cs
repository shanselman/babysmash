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
            versionLabel.Text = "Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "2.0.0";
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
            Process.Start(new ProcessStartInfo("http://feedback.babysmash.com") { UseShellExecute = true });
        }
    }
}