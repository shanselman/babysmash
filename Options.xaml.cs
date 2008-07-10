using System.Windows;
using BabySmash.Properties;

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
    }
}