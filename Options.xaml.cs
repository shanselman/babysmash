using System.Windows;
using BabySmash.Properties;
using System.Diagnostics;
using System.Deployment.Application;
using System.Windows.Controls;
using System;

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
                  try
                  {

                     deployment.UpdateProgressChanged += deployment_UpdateProgressChanged;
                     deployment.UpdateCompleted += deployment_UpdateCompleted;

                     Canvas.SetZIndex(updateProgress, (int)9999);
                     Canvas.SetZIndex(updateButton, (int)1);
                     updateProgress.Value = 0;
                     updateButton.Visibility = Visibility.Collapsed;
                     updateProgress.Visibility = Visibility.Visible;
                     updateButton.IsEnabled = false;
                     okButton.IsEnabled = false;

                     deployment.UpdateAsync();
                  }
                  catch (Exception)
                  {
                     MessageBox.Show("Sorry, but an error has occurred while updating. Please try again or contact us a http://feedback.babysmash.com. We're still learning!",
                        "Application Updater", MessageBoxButton.OK);
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

      void deployment_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
      {
         MessageBoxResult res2 = MessageBoxResult.None;
         if (e.Error == null)
         {
            res2 = MessageBox.Show("Update complete, do you want to restart the application to apply the update?",
               "Application Updater", MessageBoxButton.YesNo);
         }
         else
         {
            MessageBox.Show("Sorry, but an error has occured while updating. Please try again or contact us a http://feedback.babysmash.com. We're still learning!",
               "Application Updater", MessageBoxButton.OK);
         }
         if (res2 == MessageBoxResult.Yes)
         {
            // Restart the application to apply the update
            Application.Current.Shutdown();
            System.Windows.Forms.Application.Restart();
         }
         Canvas.SetZIndex(updateProgress, (int)1);
         Canvas.SetZIndex(updateButton, (int)9999);
         updateButton.Visibility = Visibility.Visible;
         updateProgress.Visibility = Visibility.Collapsed;
         updateButton.IsEnabled = true;
         okButton.IsEnabled = true;
      }

      void deployment_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
      {
         this.updateProgress.Value = e.ProgressPercentage;
      }
   }
}