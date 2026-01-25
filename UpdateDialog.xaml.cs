using System.Windows;

namespace BabySmash
{
    public partial class UpdateDialog : Window
    {
        public enum UpdateDialogResult
        {
            Download,
            RemindLater,
            Skip
        }

        public UpdateDialogResult Result { get; private set; } = UpdateDialogResult.RemindLater;

        public UpdateDialog(string version, string releaseNotes)
        {
            InitializeComponent();
            VersionText.Text = $"Version {version} is ready to install";
            ReleaseNotesText.Text = string.IsNullOrWhiteSpace(releaseNotes) 
                ? "No release notes available." 
                : releaseNotes;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Result = UpdateDialogResult.Download;
            DialogResult = true;
            Close();
        }

        private void RemindLaterButton_Click(object sender, RoutedEventArgs e)
        {
            Result = UpdateDialogResult.RemindLater;
            DialogResult = false;
            Close();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            Result = UpdateDialogResult.Skip;
            DialogResult = false;
            Close();
        }
    }
}
