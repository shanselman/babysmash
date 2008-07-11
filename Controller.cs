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

namespace BabySmash
{
    public class Controller
    {
        private bool isDrawing = false;
        private readonly SpeechSynthesizer objSpeech = new SpeechSynthesizer();
        private readonly List<MainWindow> windows = new List<MainWindow>();
        private int FiguresCount = 0;
        private readonly Win32Audio audio = new Win32Audio();

        public void Launch()
        {
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
                                       Topmost = true
                                   };
                m.Show();

                //TODO: Start - COMMENT IN for Debugging
                //m.Width = 700;
                //m.Height = 600;
                //m.Left = 900;
                //m.Top = 500;
                //TODO: END - COMMENT IN for Debugging

                //TODO: Start - COMMENT OUT for Debugging
                m.WindowState = WindowState.Maximized;
                //TODO: Start - COMMENT OUT for Debugging
                windows.Add(m);
            }
            ShowOptionsDialog();
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

            if (FiguresCount == Settings.Default.ClearAfter)
                ClearAllFigures();

            string s = e.Key.ToString();
            if (s.Length == 2 && s[0] == 'D') s = s[1].ToString(); //HACK: WTF? Numbers start with a "D?" as in D1?
            AddFigure(uie, s);
        }

        private void ClearAllFigures()
        {
            foreach (MainWindow m in this.windows)
            {
                m.mainCanvas.Children.Clear();
            }
        }

        private void AddFigure(FrameworkElement uie, string s)
        {
            FigureTemplate template = FigureGenerator.GenerateFigureTemplate(s);
            foreach (MainWindow m in this.windows)
            {
                UserControl f = FigureGenerator.NewUserControlFrom(template);
                m.AddUserControl(f); 

                f.Width = 300;
                f.Height = 300;
                Canvas.SetLeft(f, Utils.RandomBetweenTwoNumbers(0, Convert.ToInt32(m.ActualWidth - f.Width)));
                Canvas.SetTop(f, Utils.RandomBetweenTwoNumbers(0, Convert.ToInt32(m.ActualHeight - f.Height)));

                Storyboard storyboard = Animation.CreateDPAnimation(uie, f,
                        UIElement.OpacityProperty,
                        new Duration(TimeSpan.FromSeconds(Settings.Default.FadeAfter)));
                if (Settings.Default.FadeAway) storyboard.Begin(uie);

                IHasFace face = f as IHasFace;
                if (face != null)
                {
                    face.FaceVisible = Settings.Default.FacesOnShapes ? Visibility.Visible : Visibility.Hidden;
                }
                f.MouseLeftButtonDown += HandleMouseLeftButtonDown;
                f.MouseEnter += f_MouseEnter;
                f.MouseWheel += f_MouseWheel;
            }
            FiguresCount++;
            PlaySound(template);
        }

        void f_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(lastEnteredUserControl != null)
            {
                if (e.Delta < 0)
                {
                    Animation.ApplyZoom(lastEnteredUserControl, new Duration(TimeSpan.FromSeconds(0.5)), 2.5);
                }
                else
                {
                    Animation.ApplyZoom(lastEnteredUserControl, new Duration(TimeSpan.FromSeconds(0.5)), 0.5);
                }
            }
        }

        private UserControl lastEnteredUserControl;
        void f_MouseEnter(object sender, MouseEventArgs e)
        {
            lastEnteredUserControl = sender as UserControl;
        }

        void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserControl f = sender as UserControl;
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

            objSpeech.SpeakAsyncCancelAll();
            objSpeech.Rate = -1;
            objSpeech.Volume = 100;
            objSpeech.SpeakAsync(s);
        }

        private void ShowOptionsDialog()
        {
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
            }
        }

        public void MouseDown(MainWindow main, MouseButtonEventArgs e)
        {
            if (isDrawing || Settings.Default.MouseDraw) return;

            // Create a new Ellipse object and add it to canvas.
            Point ptCenter = e.GetPosition(main.mainCanvas);
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
            if (Settings.Default.MouseDraw && main.IsMouseCaptured == false)
                main.CaptureMouse();

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
            //sanity check
            if (main.mainCanvas.Children.Count > 300) main.mainCanvas.Children.Clear();

            //randomize at some point?
            Shape shape = new Ellipse
            {
                Stroke = SystemColors.WindowTextBrush,
                StrokeThickness = 3,
                Fill = Utils.GetGradientBrush(Utils.GetRandomColor()),
                Width = 50,
                Height = 50
            };
            main.mainCanvas.Children.Add(shape);
            Canvas.SetLeft(shape, p.X - 25);
            Canvas.SetTop(shape, p.Y - 25);

            if (Settings.Default.MouseDraw)
                audio.PlayWavResourceYield(".Resources.Sounds." + "smallbumblebee.wav");
        }

        public void LostMouseCapture(MainWindow main, MouseEventArgs e)
        {
            if (Settings.Default.MouseDraw) return;
            if (isDrawing) isDrawing = false;
        }
    }
}
