using System.Windows;
using BabySmash.Properties;
using System.Diagnostics;
using System.Deployment.Application;

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

      private void FeedbackLink_Click(object sender, RoutedEventArgs e)
      {
         Process.Start("http://feedback.babysmash.com");
      }

      private void updateButton_Click(object sender, RoutedEventArgs e)
      {
         // First check to see if we are running in a ClickOnce context
         if (ApplicationDeployment.IsNetworkDeployed)
         {
            // Get an instance of the deployment
            ApplicationDeployment deployment = ApplicationDeployment.CurrentDeployment;
            // Check to see if updates are available
            if (deployment.CheckForUpdate())
            {
               MessageBoxResult res = MessageBox.Show("A new version of the application is available, do you want to update? This will likely take a few minutes...",
                  "Application Updater",  MessageBoxButton.YesNo);
               if (res == MessageBoxResult.Yes)
               {
                  // Do the update
                  deployment.Update();
                  MessageBoxResult res2 = MessageBox.Show("Update complete, do you want to restart the application to apply the update?",
                     "Application Updater", MessageBoxButton.YesNo);
                  if (res2 == MessageBoxResult.Yes)
                  {
                     // Restart the application to apply the update
                     Application.Current.Shutdown();
                     System.Windows.Forms.Application.Restart();
                  }
               }
            }
            else
            {
               MessageBox.Show("No updates available.", "Application Updater");
            }
         }
         else
         {
            MessageBox.Show("Updates not allowed unless you are launched through ClickOnce from http://www.babysmash.com!");
         }

      }
   }
}