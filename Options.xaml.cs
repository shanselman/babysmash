using System.Windows;

namespace BabySmash
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        IsolatedStorage.ConfigurationManager config = null;
        public Options()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            config.Write("ClearAfter", txtClearAfter.Text);
            config.Write("ForceUppercase", chkForceUppercase.IsChecked.ToString());
            config.Write("Sounds", cmbSound.Text);
            config.Persist();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            config = IsolatedStorage.ConfigurationManager.GetConfigurationManager("BabySmash");
            txtClearAfter.Text = config.ReadInteger("ClearAfter", 20).ToString();
            chkForceUppercase.IsChecked = config.ReadBoolean("ForceUppercase", true);
            cmbSound.Text = config.Read("Sounds", "Laughter");
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
