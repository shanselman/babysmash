using System.ComponentModel;
using System.Windows;
using Updatum;

namespace BabySmash
{
    public partial class DownloadProgressDialog : Window
    {
        private readonly UpdatumManager _updater;

        public DownloadProgressDialog(UpdatumManager updater)
        {
            InitializeComponent();
            _updater = updater;
            _updater.PropertyChanged += UpdaterOnPropertyChanged;
        }

        private void UpdaterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UpdatumManager.DownloadedPercentage))
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.Value = _updater.DownloadedPercentage;
                    ProgressText.Text = $"{_updater.DownloadedMegabytes:F2} MB / {_updater.DownloadSizeMegabytes:F2} MB ({_updater.DownloadedPercentage:F1}%)";
                });
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _updater.PropertyChanged -= UpdaterOnPropertyChanged;
            base.OnClosing(e);
        }
    }
}
