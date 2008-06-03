using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SpeechLib;
using System.Threading;
// File, FileStream, StreamWriter
// IsolatedStorageFile
// CodeAccesPermission, IsolatedStorageFileStream
// FileIOPermission, FileIOPermissionAccess

namespace BabySmash
{
    public partial class Window1 : Window
    {
        SpVoice objSpeech = null;
        int numberOfShapes = 0;
        public Window1()
        {
            objSpeech = new SpVoice();
            InitializeComponent();
        }

        private void Window_AccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            bool Alt = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Alt) != 0;
            bool Control = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Control) != 0;
            bool Shift = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Shift) != 0;
            Trace.WriteLine(e.Key);
            if (Alt && e.Key == " ")
            {
                //trying, unsuccesfully to stop system keys
                e.Handled = true;
                return;
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
        }

        //TODO: Do I need multiple?
        Random r = new Random();

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //HACK: Why did I find the Windows Forms modifier keys classes to be so much more accurate?
            bool Alt = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Alt) != 0;
            bool Control = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Control) != 0;
            bool Shift = (System.Windows.Forms.Control.ModifierKeys & WinForms.Keys.Shift) != 0;


            if (Alt && Control && Shift && e.Key == Key.O)
            {
                e.Handled = true;
                Options o = new Options();
                o.ShowDialog();
                return;
            }

            numberOfShapes++;
            if (numberOfShapes > Properties.Settings.Default.ClearAfter)
            {
                numberOfShapes = 1;
                MainCanvas.Children.Clear();
            }

            PlayLaughter();

            string s = e.Key.ToString();
            if (s.Length == 2 && s[0] == 'D') s = s[1].ToString(); //HACK: WTF? Numbers start with a "D?"
            if (s.Length == 1 && Char.IsLetterOrDigit(s[0]))  
            {
                SpeakString(s);
                Brush brush = Utils.GetRandomColoredBrush();
                //HACK: These numbers are arbitrary and that feels gross. 
                // I'm trying to get the letters to fill the entire screen 
                // without clipping off the edge.
                double left = (double)Utils.RandomBetweenTwoNumbers(-100, Convert.ToInt32(MainCanvas.ActualWidth) - 200);
                double top = (double)Utils.RandomBetweenTwoNumbers(-100, Convert.ToInt32(MainCanvas.ActualHeight) - 300);
                //Text is Uppercase if that option is set, otherwise, it'll be random mixed case 
                MainCanvas.Children.Add(Utils.DrawCharacter(400, Properties.Settings.Default.ForceUppercase ? s: (Utils.GetRandomBoolean() ? s : s.ToLowerInvariant()), brush, left, top));

            }
            else //draw shapes
            {
                Shape shape = GetRandomShape();
                if (shape != null)
                {
                    SpeakShape(shape);

                    //TODO: Duplicated
                    double left = (double)Utils.RandomBetweenTwoNumbers(-100, Convert.ToInt32(MainCanvas.ActualWidth) - 200);
                    double top = (double)Utils.RandomBetweenTwoNumbers(-100, Convert.ToInt32(MainCanvas.ActualHeight) - 300);
                    shape.SetValue(Canvas.LeftProperty, left);
                    shape.SetValue(Canvas.TopProperty, top);
                    MainCanvas.Children.Add(shape);
                }
            }
        }

        private void PlayLaughter()
        {
            if (Properties.Settings.Default.Sounds == "Laughter")
            {
                Winmm.PlayWavResource(Utils.GetRandomSoundFile());
            }
        }

        private void SpeakString(string s)
        {
            if (Properties.Settings.Default.Sounds == "Speech")
            {
                objSpeech.WaitUntilDone(Timeout.Infinite);
                objSpeech.Speak(s, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            }
        }

        private void SpeakShape(Shape s)
        {
            //TODO: Make this more OO...perhaps keep a Dictionary<type, string>?
            if (s is Ellipse)
            {
                SpeakString("circle");
            }
            else if (s is Star)
            {
                SpeakString("star");
            }
            else if (s is Polygon)
            {
                SpeakString("triangle");
            }
            else if (s is Rectangle)
            {
                SpeakString("rectangle");
            }
        }

        private Shape GetRandomShape()
        {
            //TODO: Should this be in XAML? Would that make it better?
            Shape retVal = null;
            Brush fill = Utils.GetRandomColoredBrush();
            int shape = Convert.ToInt32(Utils.RandomBetweenTwoNumbers(0, 3));
            //TODO: Should I change the height, width and stroke to be relative to the screen size?
            //TODO: I think I need a shapefactory?
            switch (shape)
            {
                case 0:
                    retVal = new Rectangle()
                    {
                        Fill = fill,
                        Height = 380,
                        Width = 380,
                        StrokeThickness = 5,
                        Stroke = Brushes.Black,
                    };

                   break;

                case 1:
                   retVal = new Ellipse()
                   {
                        Fill = fill,
                       Height = 400,
                       Width = 400,
                       StrokeThickness = 5,
                       Stroke = Brushes.Black,
                   };

                   break;
                case 2:
                   retVal = new Polygon()
                   {
                       Points = new PointCollection(new Point[]{
                            new Point(200,50), 
                            new Point(400,400), 
                            new Point(0,400), 
                            new Point(200,50)}),
                       Height = 400,
                       Width = 400,
                       Fill = fill,
                       StrokeThickness = 5,
                       Stroke = Brushes.Black,
                   };
                   break;
                case 3:
                   retVal = new Star()
                   {
                       NumberOfPoints = 5,
                       Height = 400,
                       Width = 400,
                       Fill = fill,
                       StrokeThickness = 5,
                       Stroke = Brushes.Black,
                   };
                   break;
            }
            return retVal;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: Make the link an actual Hyperlink
            TextBlock b = new TextBlock()
            {
                Text = "BabySmash by Scott Hanselman http://www.hanselman.com\r\nCtrl-Shift-Alt-O for options, ALT-F4 to exit",
            };
            MainCanvas.Children.Add(b);
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO: This only works if I close the window on the primary monitor. 
            // How do close them all if I close a secondary monitor's window?
            foreach (Window w in this.OwnedWindows)
            {
                w.Close();
            }
        }
   }
}
