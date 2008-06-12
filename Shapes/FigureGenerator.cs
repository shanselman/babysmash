using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BabySmash
{
   public class FigureGenerator 
   {
      private int clearAfter;
      private readonly ObservableCollection<Figure> figures = new ObservableCollection<Figure>();
      
      public int ClearAfter
      {
         get { return clearAfter; }
         set { clearAfter = value; }
      }

      public ObservableCollection<Figure> Figures
      {
         get { return figures; }
      }

      public void Generate(FrameworkElement container, string letter)
      {
         if (figures.Count == clearAfter)
            figures.Clear();
         Figure f = GenerateFigure(letter);
         Storyboard s = CreateStoryboardAnimation(container, f.Shape, Shape.OpacityProperty);
         figures.Add(f);
         if (Properties.Settings.Default.FadeAway) s.Begin(container);
      }

      private Storyboard CreateStoryboardAnimation(FrameworkElement container, UIElement shape, DependencyProperty dp)
      {
         Storyboard st = new Storyboard();
         NameScope.SetNameScope(container, new NameScope());
         container.RegisterName("shape", shape);

         DoubleAnimation d = new DoubleAnimation();
         d.From = 1.0;
         d.To = 0.0;
         d.Duration = new Duration(TimeSpan.FromSeconds(5));
         d.AutoReverse = false;

         st.Children.Add(d);
         Storyboard.SetTargetName(d, "shape");
         Storyboard.SetTargetProperty(d, new PropertyPath(dp));
         return st;
      }

      private Figure GenerateFigure(string letter)
      {
         //TODO: Should this be in XAML? Would that make it better?
         Brush fill = Utils.GetRandomColoredBrush();
         if (letter.Length == 1 && Char.IsLetterOrDigit(letter[0]))
         {
            return new LetterFigure(fill, letter);
         }
         else
         {
            int shape = Utils.RandomBetweenTwoNumbers(0, 6);
            //TODO: Should I change the height, width and stroke to be relative to the screen size?
            //TODO: I think I need a shapefactory?
            switch (shape)
            {
               case 0:
                  return new SquareFigure(fill);
               case 1:
                  return new CircleFigure(fill);
               case 2:
                  return new TriangleFigure(fill);
               case 3:
                  return new StarFigure(fill);
               case 4:
                  return new HeartFigure(fill);
               case 5:
                  return new TrapezoidFigure(fill);
               case 6:
                  return new RectangleFigure(fill);
            }
         }
         return null;
      }

   }
}
