using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BabySmash
{
   public abstract class Figure
   {
      private UIElement shape;
      private readonly string name;
      private readonly string color;

      protected Figure(Brush fill, string name)
      {
         this.color = Utils.BrushToString(fill);
         this.name = name;
      }

      public UIElement Shape
      {
         get { return shape; }
         protected set { shape = value; }
      }

      public string Name { get { return name; } }
      public string Color { get { return color; } }
   }

   public class LetterFigure : Figure
   {
      public LetterFigure(Brush fill, string name)
         : base(fill, name)
      {
         string nameToDisplay;
         if (Properties.Settings.Default.ForceUppercase)
         {
            nameToDisplay = name;
         }
         else
         {
            if (Utils.GetRandomBoolean())
               nameToDisplay = name;
            else
               nameToDisplay = name.ToLowerInvariant();
         }
         Shape = DrawCharacter(400, nameToDisplay, fill);
      }

      private UIElement DrawCharacter(double fontSize, string textString, Brush brush)
      {
         HiResTextBlock textBlock = new HiResTextBlock()
         {
            FontSize = fontSize, //pick better size
            Fill = brush,
            Text = textString,
            StrokeThickness = 5,
         };
         return textBlock;
      }

   }

   public class SquareFigure : Figure
   {
      public SquareFigure(Brush fill)
         : base(fill, "square")
      {
         Shape = new Rectangle()
         {
            Fill = fill,
            Height = 380,
            Width = 380,
            StrokeThickness = 5,
            Stroke = Brushes.Black,
         };
      }
   }

   public class RectangleFigure : Figure
   {
      public RectangleFigure(Brush fill)
         : base(fill, "rectangle")
      {
         Shape = new Rectangle()
         {
            Fill = fill,
            Height = 160,
            Width = 380,
            StrokeThickness = 5,
            Stroke = Brushes.Black,
         };
      }
   }

   public class CircleFigure : Figure
   {
      public CircleFigure(Brush fill)
         : base(fill, "circle")
      {
         Shape = new Ellipse()
         {
            Fill = fill,
            Height = 400,
            Width = 400,
            StrokeThickness = 5,
            Stroke = Brushes.Black,
         };
      }
   }

   public class TriangleFigure : Figure
   {
      public TriangleFigure(Brush fill)
         : base(fill, "triangle")
      {
         Shape = new Polygon()
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
      }
   }

   public class StarFigure : Figure
   {
      public StarFigure(Brush fill)
         : base(fill, "star")
      {
         Shape = new Star()
         {
            NumberOfPoints = 5,
            Height = 400,
            Width = 400,
            Fill = fill,
            StrokeThickness = 5,
            Stroke = Brushes.Black,
         };
      }
   }

   public class TrapezoidFigure : Figure
   {
      public TrapezoidFigure(Brush fill)
         : base(fill, "trapezoid")
      {
         Shape = new Path()
         {
            Data = Geometry.Parse("F1 M 257.147,126.953L 543.657,126.953L 640.333,448.287L 160.333,448.287L 257.147,126.953 Z "),
            Fill = fill,
            Stroke = Brushes.Black,
            StrokeThickness = 5,
         };
      }
   }

   public class HeartFigure : Figure
   {
      public HeartFigure(Brush fill)
         : base(fill, "heart")
      {
         Shape = new Path()
         {
            Data = Geometry.Parse("F1 M 429,161.333C 506.333,88.6667 609,142.122 609,225.333C 609,308.544 429,462.667 429,462.667C 429,462.667 257,306.544 257,223.333C 257,140.123 356.138,88.4713 429,161.333 Z "),
            Fill = fill,
            Stroke = Brushes.Black,
            StrokeThickness = 5,
         };
      }

   }
}