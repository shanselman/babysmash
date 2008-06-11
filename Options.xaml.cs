using System.Windows;

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
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();
            this.Close();
        }
    }
}


