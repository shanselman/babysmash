using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Speech.Synthesis;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace BabySmash
{
   public partial class MainWindow : Window
   {
      private SpeechSynthesizer objSpeech;
      private readonly FigureGenerator figureGenerator;

      public MainWindow()
      {
         InitializeComponent();
         figureGenerator = (FigureGenerator)Resources["figureGenerator"];
         figureGenerator.ClearAfter = Properties.Settings.Default.ClearAfter;
         ICollectionView collectionView = CollectionViewSource.GetDefaultView(figureGenerator.Figures);
         collectionView.CollectionChanged += FiguresCollectionChanged;
      }

      private void FiguresCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if (e.Action == NotifyCollectionChangedAction.Add)
         {
            Figure f = (Figure)e.NewItems[0];
            PlayLaughter();
            if (f is LetterFigure)
               SpeakString(f.Name);
            else
               SpeakString(f.Color + " " + f.Name);
         }
      }

      public double ShapeLeft
      {
         //TODO: Feels weird...we need to know the actualwidth of the canvas and of the figure....
         // ideally: Utils.RandomBetweenTwoNumbers(0, 
         //   Convert.ToInt32(canvas.ActualWidth - figure.ActualWidth)
         // Perhaps this belongs elsewhere?
         get { return Utils.RandomBetweenTwoNumbers(-100, 
             Convert.ToInt32(figures.ActualWidth) - 200); }
      }

      public double ShapeTop
      {
         //TODO: Feels weird...we need to know the actualwidth of the canvas and of the figure....
         // ideally: Utils.RandomBetweenTwoNumbers(0, 
         // Convert.ToInt32(canvas.ActualWidth - figure.ActualWidth)
         // Perhaps this belongs elsewhere?
         get { return Utils.RandomBetweenTwoNumbers(
             -100, 
             Convert.ToInt32(figures.ActualHeight) - 300); }
      }

      private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
      {
         //HACK: Why did I find the Windows Forms modifier keys classes to be so much more accurate?
         bool Alt = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
         bool Control = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
         bool Shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

         //TODO: Might be able to remove this: http://www.ageektrapped.com/blog/using-commands-in-babysmash/
         if (Alt && Control && Shift && e.Key == Key.O)
         {
            var o = new Options();
            o.ShowDialog();
            e.Handled = true;
            return;
         }

         string s = e.Key.ToString();
         if (s.Length == 2 && s[0] == 'D') s = s[1].ToString(); //HACK: WTF? Numbers start with a "D?" as in D1?
         figureGenerator.Generate(this, s);
      }

      private static void PlayLaughter()
      {
         if (Properties.Settings.Default.Sounds == "Laughter")
         {
            Winmm.PlayWavResource(Utils.GetRandomSoundFile());
         }
      }

      private void SpeakString(string s)
      {
          if (objSpeech == null || Properties.Settings.Default.Sounds != "Speech") return;
          
          objSpeech.SpeakAsyncCancelAll();
          objSpeech.Rate = -1;
          objSpeech.Volume = 100;
          objSpeech.SpeakAsync(s);
      }

      private void HelpUrlNavigated(object sender, RequestNavigateEventArgs e)
      {
         Process.Start(e.Uri.ToString());
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         objSpeech = new SpeechSynthesizer();
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         Application.Current.Shutdown();
      }
   }
}
