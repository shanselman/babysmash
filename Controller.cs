using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BabySmash.Properties;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using WinForms = System.Windows.Forms;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Deployment.Application;
using System.Windows.Media;
using System.Threading;
using System.ComponentModel;

namespace BabySmash
{
   public class Controller
   {
      [DllImport("user32.dll")]
      private static extern IntPtr SetFocus(IntPtr hWnd);

      [DllImport("user32.dll")]
      private static extern bool SetForegroundWindow(IntPtr hWnd);

      public bool isOptionsDialogShown { get; set; }
      private bool isDrawing = false;
      private readonly SpeechSynthesizer objSpeech = new SpeechSynthesizer();
      private readonly List<MainWindow> windows = new List<MainWindow>();
      private readonly Win32Audio audio = new Win32Audio();
      private DispatcherTimer timer = new DispatcherTimer();
      private Queue<Shape> ellipsesQueue = new Queue<Shape>();
      private Dictionary<string,Queue<UserControl>> ellipsesUserControlQueue = new Dictionary<string,Queue<UserControl>>();
      private ApplicationDeployment deployment = null;

      void deployment_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
      {
         if (e.Error == null && e.UpdateAvailable)
         {
            try
            {
               MainWindow w = this.windows[0];
               w.updateProgress.Value = 0;
               w.UpdateAvailableLabel.Visibility = Visibility.Visible;

               deployment.UpdateAsync();
            }
            catch (InvalidOperationException ex)
            {
               Debug.WriteLine(ex.ToString());
               MainWindow w = this.windows[0];
               w.UpdateAvailableLabel.Visibility = Visibility.Hidden;
            }
         }
      }
      
      void deployment_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
      {
         MainWindow w = this.windows[0];
         w.updateProgress.Value = e.ProgressPercentage;
      }

      void deployment_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
      {
         if (e.Error != null)
         {
            Debug.WriteLine(e.ToString());
            return;
         }
         MainWindow w = this.windows[0];
         w.UpdateAvailableLabel.Visibility = Visibility.Hidden;
      }

      public void Launch()
      {
         timer.Tick += new EventHandler(timer_Tick);
         timer.Interval = new TimeSpan(0, 0, 1);
         int Number = 0;

         if (ApplicationDeployment.IsNetworkDeployed)
         {
            deployment = ApplicationDeployment.CurrentDeployment;
            deployment.UpdateCompleted += new System.ComponentModel.AsyncCompletedEventHandler(deployment_UpdateCompleted);
            deployment.UpdateProgressChanged += deployment_UpdateProgressChanged;
            deployment.CheckForUpdateCompleted += deployment_CheckForUpdateCompleted;
            try
            {
               deployment.CheckForUpdateAsync();
            }
            catch (InvalidOperationException e)
            {
               Debug.WriteLine(e.ToString());
            }
         }

         foreach (WinForms.Screen s in WinForms.Screen.AllScreens)
         {
            MainWindow m = new MainWindow(this)
                               {
                                  WindowStartupLocation = WindowStartupLocation.Manual,
                                  Left = s.WorkingArea.Left,
                                  Top = s.WorkingArea.Top,
                                  Width = s.WorkingArea.Width,
                                  Height = s.WorkingArea.Height,
                                  WindowStyle = WindowStyle.None,
                                  Topmost = true,
                                  AllowsTransparency = Settings.Default.TransparentBackground,
	                          Background = (Settings.Default.TransparentBackground ? new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)) : Brushes.WhiteSmoke),
                                  Name = "Window" + Number++.ToString()
 				};



            ellipsesUserControlQueue[m.Name] = new Queue<UserControl>();

            m.Show();
            m.MouseLeftButtonDown += HandleMouseLeftButtonDown;
            m.MouseWheel += HandleMouseWheel;

#if false
            m.Width = 700;
            m.Height = 600;
            m.Left = 900;
            m.Top = 500;
#else
            m.WindowState = WindowState.Maximized;
#endif
            windows.Add(m);
         }

	    //Only show the info label on the FIRST monitor.
         windows[0].infoLabel.Visibility = Visibility.Visible;

         //Startup sound
         audio.PlayWavResourceYield(".Resources.Sounds." + "EditedJackPlaysBabySmash.wav");

         string[] args = Environment.GetCommandLineArgs();
         string ext = System.IO.Path.GetExtension(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

         if (ApplicationDeployment.IsNetworkDeployed && (ApplicationDeployment.CurrentDeployment.IsFirstRun || ApplicationDeployment.CurrentDeployment.UpdatedVersion != ApplicationDeployment.CurrentDeployment.CurrentVersion))            
         {
            //if someone made us a screensaver, then don't show the options dialog.
            if ((args != null && args[0] != "/s") && String.CompareOrdinal(ext, ".SCR") != 0)
            {
               ShowOptionsDialog();
            }
         }
#if !false
         timer.Start();
#endif
      }

      void timer_Tick(object sender, EventArgs e)
      {
         if (isOptionsDialogShown) return;

         try
         {
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            SetForegroundWindow(windowHandle);
            SetFocus(windowHandle);
         }
         catch (Exception)
         {
            //Wish me luck!
         }
      }

      public void ProcessKey(FrameworkElement uie, KeyEventArgs e)
      {
         bool Alt = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
         bool Control = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
         bool Shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

         if (uie.IsMouseCaptured)
            uie.ReleaseMouseCapture();

         //TODO: Might be able to remove this: http://www.ageektrapped.com/blog/using-commands-in-babysmash/
         if (Alt && Control && Shift && e.Key == Key.O)
         {
            ShowOptionsDialog();
            e.Handled = true;
            return;
         }

         string s = e.Key.ToString();
         if (s.Length == 2 && s[0] == 'D') s = s[1].ToString(); //HACK: WTF? Numbers start with a "D?" as in D1?
         AddFigure(uie, s);
      }

      private void AddFigure(FrameworkElement uie, string s)
      {
         FigureTemplate template = FigureGenerator.GenerateFigureTemplate(s);
         foreach (MainWindow m in this.windows)
         {
            UserControl f = FigureGenerator.NewUserControlFrom(template);
            m.AddFigure(f);

            var queue = ellipsesUserControlQueue[m.Name];
            queue.Enqueue(f);

            f.Width = 300;
            f.Height = 300;
            Canvas.SetLeft(f, Utils.RandomBetweenTwoNumbers(0, Convert.ToInt32(m.ActualWidth - f.Width)));
            Canvas.SetTop(f, Utils.RandomBetweenTwoNumbers(0, Convert.ToInt32(m.ActualHeight - f.Height)));

            Storyboard storyboard = Animation.CreateDPAnimation(uie, f,
                    UIElement.OpacityProperty,
                    new Duration(TimeSpan.FromSeconds(Settings.Default.FadeAfter)),1,0);
            if (Settings.Default.FadeAway) storyboard.Begin(uie);

            IHasFace face = f as IHasFace;
            if (face != null)
            {
               face.FaceVisible = Settings.Default.FacesOnShapes ? Visibility.Visible : Visibility.Hidden;
            }

            if (queue.Count > Settings.Default.ClearAfter)
            {
               UserControl u = queue.Dequeue() as UserControl;
               m.RemoveFigure(u);
            }
         }

         PlaySound(template);
      }

      //private static DoubleAnimationUsingKeyFrames ApplyZoomOut(UserControl u)
      //{
      //   Tweener.TransitionType rt1 = Tweener.TransitionType.EaseOutExpo;
      //   var ani1 = Tweener.Tween.CreateAnimation(rt1, 1, 0, TimeSpan.FromSeconds(0.5));
      //   u.RenderTransformOrigin = new Point(0.5, 0.5);
      //   var group = new TransformGroup();
      //   u.RenderTransform = group;

      //   ani1.Completed += new EventHandler(ani1_Completed); 

      //   group.Children.Add(new ScaleTransform());
      //   group.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, ani1);
      //   group.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, ani1);
      //   return ani1;
      //}

      //static void ani1_Completed(object sender, EventArgs e)
      //{
      //   AnimationClock clock = sender as AnimationClock;
      //   Debug.Write(sender.ToString());
      //   UserControl foo = sender as UserControl;
      //   UserControl toBeRemoved = queue.Dequeue() as UserControl;
      //   Canvas container = toBeRemoved.Parent as Canvas;
      //   container.Children.Remove(toBeRemoved);
      //}

      void HandleMouseWheel(object sender, MouseWheelEventArgs e)
      {
         UserControl foo = sender as UserControl; //expected this on Sender!
         if (foo != null)
         {
            if (e.Delta < 0)
            {
               Animation.ApplyZoom(foo, new Duration(TimeSpan.FromSeconds(0.5)), 2.5);
            }
            else
            {
               Animation.ApplyZoom(foo, new Duration(TimeSpan.FromSeconds(0.5)), 0.5);
            }
         }
      }

      void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         UserControl f = e.Source as UserControl;
         if (f != null && f.Opacity > 0.1) //can it be seen? 
         {
            isDrawing = true; //HACK: This is a cheat to stop the mouse draw action.
            Animation.ApplyRandomAnimationEffect(f, Duration.Automatic);
            PlayLaughter(); //Might be better to re-speak the name, color, etc.
         }
      }

      public void PlaySound(FigureTemplate template)
      {
         if (Settings.Default.Sounds == "Laughter")
         {
            PlayLaughter();
         }
         if (objSpeech != null && Settings.Default.Sounds == "Speech")
         {
            if (template.Letter != null && template.Letter.Length == 1 && Char.IsLetterOrDigit(template.Letter[0]))
            {
               SpeakString(template.Letter);
            }
            else
            {
               SpeakString(Utils.ColorToString(template.Color) + " " + template.Name);
            }
         }
      }

      private void PlayLaughter()
      {
         audio.PlayWavResource(Utils.GetRandomSoundFile());
      }

      private void SpeakString(string s)
      {
         ThreadedSpeak ts = new ThreadedSpeak(s);
         ts.Speak();
      }

      private class ThreadedSpeak
      {
         string Word = null;
         SpeechSynthesizer SpeechSynth = new SpeechSynthesizer();
         public ThreadedSpeak(string Word)
         {
            this.Word = Word;
            SpeechSynth.Rate = -1;
            SpeechSynth.Volume = 100;
         }
         public void Speak()
         {
            System.Threading.Thread oThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.Start));
            oThread.Start();
         }
         private void Start()
         {
            try
            {
               SpeechSynth.Speak(Word);
            }
            catch (Exception e)
            {
               System.Diagnostics.Trace.WriteLine(e.ToString());
            }

         }
      }

      private void ShowOptionsDialog()
      {
         bool foo = Settings.Default.TransparentBackground;
         isOptionsDialogShown = true;
         var o = new Options();
         Mouse.Capture(null);
         foreach (MainWindow m in this.windows)
         {
            m.Topmost = false;
         }
         o.Topmost = true;
         o.Focus();
         o.ShowDialog();
         Debug.Write("test");
         foreach (MainWindow m in this.windows)
         {
            m.Topmost = true;
            //m.ResetCanvas();
         }
         isOptionsDialogShown = false;

         if (foo != Settings.Default.TransparentBackground)
         {
            MessageBoxResult result = MessageBox.Show(
                "You've changed the Window Transparency Option. We'll need to restart BabySmash! for you to see the change. Pressing YES will restart BabySmash!. Is that OK?",
                "Need to Restart", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
               Application.Current.Shutdown();
               System.Windows.Forms.Application.Restart();
            }
         }
      }

      public void MouseDown(MainWindow main, MouseButtonEventArgs e)
      {
         if (isDrawing || Settings.Default.MouseDraw) return;

         // Create a new Ellipse object and add it to canvas.
         Point ptCenter = e.GetPosition(main.mouseCursorCanvas);
         MouseDraw(main, ptCenter);
         isDrawing = true;
         main.CaptureMouse();

         audio.PlayWavResource(".Resources.Sounds." + "smallbumblebee.wav");
      }

      public void MouseWheel(MainWindow main, MouseWheelEventArgs e)
      {
         if (e.Delta > 0)
         {
            audio.PlayWavResourceYield(".Resources.Sounds." + "rising.wav");
         }
         else
         {
            audio.PlayWavResourceYield(".Resources.Sounds." + "falling.wav");
         }
      }

      public void MouseUp(MainWindow main, MouseButtonEventArgs e)
      {
         isDrawing = false;
         if (Settings.Default.MouseDraw) return;
         main.ReleaseMouseCapture();
      }

      public void MouseMove(MainWindow main, MouseEventArgs e)
      {
          if (isOptionsDialogShown)
          {
              main.ReleaseMouseCapture();
              return;
          }
          if (Settings.Default.MouseDraw && main.IsMouseCaptured == false)
            main.CaptureMouse();

          Debug.WriteLine(String.Format("MouseMove! {0} {1} {2}", Settings.Default.MouseDraw, main.IsMouseCaptured, isOptionsDialogShown));

         if (isDrawing || Settings.Default.MouseDraw)
         {
            MouseDraw(main, e.GetPosition(main));
         }

         //Cheesy, but hotkeys are ignored when the mouse is captured. 
         // However, if we don't capture and release, the shapes will draw forever.
         if (Settings.Default.MouseDraw && main.IsMouseCaptured)
            main.ReleaseMouseCapture();
      }

      private void MouseDraw(MainWindow main, Point p)
      {
         //randomize at some point?
         Shape shape = new Ellipse
         {
            Stroke = SystemColors.WindowTextBrush,
            StrokeThickness = 0,
            Fill = Utils.GetGradientBrush(Utils.GetRandomColor()),
            Width = 50,
            Height = 50
         };

         ellipsesQueue.Enqueue(shape);
         main.mouseDragCanvas.Children.Add(shape);
         Canvas.SetLeft(shape, p.X - 25);
         Canvas.SetTop(shape, p.Y - 25);

         if (Settings.Default.MouseDraw)
            audio.PlayWavResourceYield(".Resources.Sounds." + "smallbumblebee.wav");

         if (ellipsesQueue.Count > 30) //this is arbitrary
         {
            Shape shapeToRemove = ellipsesQueue.Dequeue();
            main.mouseDragCanvas.Children.Remove(shapeToRemove);
         }
      }

      //private static void ResetCanvas(MainWindow main)
      //{
      //   main.ResetCanvas();
      //}

      public void LostMouseCapture(MainWindow main, MouseEventArgs e)
      {
         if (Settings.Default.MouseDraw) return;
         if (isDrawing) isDrawing = false;
      }
   }
}
