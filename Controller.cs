using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BabySmash.Properties;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using WinForms = System.Windows.Forms;

namespace BabySmash
{
    using System.Globalization;
    using System.IO;
    using System.Speech.Synthesis;
    using System.Text;

    using Newtonsoft.Json;

    public class Controller
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static Controller instance = new Controller();

        public bool isOptionsDialogShown { get; set; }
        private bool isDrawing = false;
        private readonly SpeechSynthesizer objSpeech = new SpeechSynthesizer();
        private readonly List<MainWindow> windows = new List<MainWindow>();

        private DispatcherTimer timer = new DispatcherTimer();
        private Queue<Shape> ellipsesQueue = new Queue<Shape>();
        private Dictionary<string, List<UserControl>> figuresUserControlQueue = new Dictionary<string, List<UserControl>>();
        private ApplicationDeployment deployment = null;
        private WordFinder wordFinder = new WordFinder("Words.txt");

        /// <summary>Prevents a default instance of the Controller class from being created.</summary>
        private Controller() { }

        public static Controller Instance
        {
            get { return instance; }
        }

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
                    ResizeMode = ResizeMode.NoResize,
                    Topmost = true,
                    AllowsTransparency = Settings.Default.TransparentBackground,
                    Background = (Settings.Default.TransparentBackground ? new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)) : Brushes.WhiteSmoke),
                    Name = "Window" + Number++.ToString()
                };

                figuresUserControlQueue[m.Name] = new List<UserControl>();

                m.Show();
                m.MouseLeftButtonDown += HandleMouseLeftButtonDown;
                m.MouseWheel += HandleMouseWheel;
                m.WindowState = WindowState.Maximized;
                windows.Add(m);
            }

            //Only show the info label on the FIRST monitor.
            windows[0].infoLabel.Visibility = Visibility.Visible;

            //Startup sound
            Win32Audio.PlayWavResourceYield("EditedJackPlaysBabySmash.wav");

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
            if (isOptionsDialogShown)
            {
                return;
            }

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
            if (uie.IsMouseCaptured)
            {
                uie.ReleaseMouseCapture();
            }
            
            char displayChar = GetDisplayChar(e.Key);
            AddFigure(uie, displayChar);
        }

        private char GetDisplayChar(Key key)
        {
            // If a number on the normal number track is pressed, display the number.
            if (key >= Key.D0 && key <= Key.D9)
            {
                return (char)('0' + key - Key.D0);
            }

            // If a number on the numpad is pressed, display the number.
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                return (char)('0' + key - Key.NumPad0);
            }

            try
            {
                return char.ToUpperInvariant(TryGetLetter(key));
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                return '*';
            }
        }

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
                uint wVirtKey,
                uint wScanCode,
                byte[] lpKeyState,
                [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
                        StringBuilder pwszBuff,
                int cchBuff,
                uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        private static char TryGetLetter(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

        private void AddFigure(FrameworkElement uie, char c)
        {
            FigureTemplate template = FigureGenerator.GenerateFigureTemplate(c);
            foreach (MainWindow window in this.windows)
            {
                UserControl f = FigureGenerator.NewUserControlFrom(template);
                window.AddFigure(f);

                var queue = figuresUserControlQueue[window.Name];
                queue.Add(f);

                // Letters should already have accurate width and height, but others may them assigned.
                if (double.IsNaN(f.Width) || double.IsNaN(f.Height))
                {
                    f.Width = 300;
                    f.Height = 300;
                }

                Canvas.SetLeft(f, Utils.RandomBetweenTwoNumbers(0, Convert.ToInt32(window.ActualWidth - f.Width)));
                Canvas.SetTop(f, Utils.RandomBetweenTwoNumbers(0, Convert.ToInt32(window.ActualHeight - f.Height)));

                Storyboard storyboard = Animation.CreateDPAnimation(uie, f,
                                UIElement.OpacityProperty,
                                new Duration(TimeSpan.FromSeconds(Settings.Default.FadeAfter)), 1, 0);
                if (Settings.Default.FadeAway) storyboard.Begin(uie);

                IHasFace face = f as IHasFace;
                if (face != null)
                {
                    face.FaceVisible = Settings.Default.FacesOnShapes ? Visibility.Visible : Visibility.Hidden;
                }

                if (queue.Count > Settings.Default.ClearAfter)
                {
                    window.RemoveFigure(queue[0]);
                    queue.RemoveAt(0);
                }
            }

            // Find the last word typed, if applicable.
            string lastWord = this.wordFinder.LastWord(figuresUserControlQueue.Values.First());
            if (lastWord != null)
            {
                foreach (MainWindow window in this.windows)
                {
                    this.wordFinder.AnimateLettersIntoWord(figuresUserControlQueue[window.Name], lastWord);
                }

                SpeakString(lastWord);
            }
            else
            {
                PlaySound(template);
            }
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
                    SpeakString(GetLocalizedString(Utils.ColorToString(template.Color)) + " " + template.Name);
                }
            }
        }

        /// <summary>
        /// Returns <param name="key"></param> if value or culture is not found.
        /// </summary>
        public static string GetLocalizedString(string key)
        {
            CultureInfo keyboardLanguage = System.Windows.Forms.InputLanguage.CurrentInputLanguage.Culture;
            string culture = keyboardLanguage.Name;
            string path = $@"Resources\Strings\{culture}.json";
            string path2 = @"Resources\Strings\en-EN.json";
            string jsonConfig = null;
            if (File.Exists(path))
            {
                jsonConfig = File.ReadAllText(path);
            }
            else if (File.Exists(path2))
            {
                jsonConfig = File.ReadAllText(path2);
            }

            if (jsonConfig != null)
            {
                Dictionary<string, object> config = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonConfig);
                if (config.ContainsKey(key))
                {
                    return config[key].ToString();
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "No file");
            }

            return key;
        }

        private void PlayLaughter()
        {
            Win32Audio.PlayWavResource(Utils.GetRandomSoundFile());
        }

        private void SpeakString(string s)
        {
            ThreadedSpeak ts = new ThreadedSpeak(s);
            ts.Speak();
        }

        private class ThreadedSpeak
        {
            private string Word = null;
            SpeechSynthesizer SpeechSynth = new SpeechSynthesizer();
            public ThreadedSpeak(string Word)
            {
                this.Word = Word;
                CultureInfo keyboardLanguage = System.Windows.Forms.InputLanguage.CurrentInputLanguage.Culture;
                InstalledVoice neededVoice = this.SpeechSynth.GetInstalledVoices(keyboardLanguage).FirstOrDefault();
                if (neededVoice == null)
                {
                    //http://superuser.com/questions/590779/how-to-install-more-voices-to-windows-speech
                    //https://msdn.microsoft.com/en-us/library/windows.media.speechsynthesis.speechsynthesizer.voice.aspx
                    //http://stackoverflow.com/questions/34776593/speechsynthesizer-selectvoice-fails-with-no-matching-voice-is-installed-or-th
                    this.Word = "Unsupported Language";
                }
                else if (!neededVoice.Enabled)
                {
                    this.Word = "Voice Disabled";
                }
                else
                {
                    try
                    {
                        this.SpeechSynth.SelectVoice(neededVoice.VoiceInfo.Name);
                    }
                    catch (Exception ex)
                    {
                        Debug.Assert(false, ex.ToString());
                    }
                }

                SpeechSynth.Rate = -1;
                SpeechSynth.Volume = 100;
            }
            public void Speak()
            {
                Thread oThread = new Thread(new ThreadStart(this.Start));
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

        public void ShowOptionsDialog()
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

            Win32Audio.PlayWavResource("smallbumblebee.wav");
        }

        public void MouseWheel(MainWindow main, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Win32Audio.PlayWavResourceYield("rising.wav");
            }
            else
            {
                Win32Audio.PlayWavResourceYield("falling.wav");
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

            if (isDrawing || Settings.Default.MouseDraw)
            {
                MouseDraw(main, e.GetPosition(main));
            }

            // Cheesy, but hotkeys are ignored when the mouse is captured.
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
                Win32Audio.PlayWavResourceYield("smallbumblebee.wav");

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