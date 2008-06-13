using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Drawing = System.Drawing;
using System.Globalization;

namespace BabySmash
{
   public abstract class Figure
   {
      private readonly string name;
      private readonly string color;

      protected Figure(Brush fill, string name, Shape s)
      {
         this.color = Utils.BrushToString(fill);
         this.name = name;
         this.Shape = s;
         s.Fill = fill;
         s.Style = Application.Current.Resources[Name] as Style;
      }

      public UIElement Shape { get; protected set; }

      public string Name { get { return name; } }
      public string Color { get { return color; } }
   }

   public class SquareFigure : Figure
   {
       public SquareFigure(Brush fill)
           : base(fill, "square", new Rectangle()){}
   }

   public class RectangleFigure : Figure
   {
      public RectangleFigure(Brush fill)
         : base(fill, "rectangle", new Rectangle()){}
   }

   public class CircleFigure : Figure
   {
      public CircleFigure(Brush fill)
         : base(fill, "circle", new Ellipse()){}
   }

   public class TriangleFigure : Figure
   {
      public TriangleFigure(Brush fill)
         : base(fill, "triangle", new Polygon()){}
   }

   public class StarFigure : Figure
   {
      public StarFigure(Brush fill)
         : base(fill, "star", new Star()){}
   }

   public class TrapezoidFigure : Figure
   {
      public TrapezoidFigure(Brush fill)
         : base(fill, "trapezoid", new Path())
      {
      }
   }

   public class HeartFigure : Figure
   {
      public HeartFigure(Brush fill)
         : base(fill, "heart", new Path())
      {}
   }

   public class LetterFigure : Figure
   {
       public LetterFigure(Brush fill, string name)
           : base(fill, "BabySmashBaseStyle",
            new Path()
            {
                Data = MakeCharacterGeometry(
                    GetLetterCharacter(name)),
                Height = 400
            }
          )
       {
       }

       private static string GetLetterCharacter(string name)
       {
           string nameToDisplay;
           if (Properties.Settings.Default.ForceUppercase)
           {
               nameToDisplay = name;
           }
           else
           {
               nameToDisplay = Utils.GetRandomBoolean() ? name : name.ToLowerInvariant();
           }
           return nameToDisplay;
       }

       private static Geometry MakeCharacterGeometry(string t)
       {
           FormattedText fText = new FormattedText(
              t,
              CultureInfo.CurrentCulture,
              FlowDirection.LeftToRight,
              new Typeface(
                  new FontFamily("Arial"),
                  FontStyles.Normal,
                  FontWeights.Heavy,
                  FontStretches.Normal),
              300,
              Brushes.Black
              );

           return fText.BuildGeometry(new Point(0, 0)).GetAsFrozen() as Geometry;
       }
   }
}


