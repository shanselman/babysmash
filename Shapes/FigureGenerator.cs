using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using BrushControlFunc = System.Func<System.Windows.Media.Brush, System.Windows.Controls.UserControl>;
using System.Windows;

namespace BabySmash
{
   public class FigureTemplate
   {
      public Brush Fill { get; set; }
      public Color Color { get; set; }
      public BrushControlFunc GeneratorFunc { get; set; }
      public BitmapEffect Effect { get; set; }
      public string Name { get; set; }
      public string Letter { get; set; }
   }

   public class FigureGenerator
   {
      private static readonly List<KeyValuePair<string, BrushControlFunc>> hashTableOfFigureGenerators = new List<KeyValuePair<string, BrushControlFunc>>
       {
           new KeyValuePair<string, BrushControlFunc>("Circle", x => new CoolCircle(x) ),
           new KeyValuePair<string, BrushControlFunc>("Rectangle", x => new CoolRectangle(x) ),
           new KeyValuePair<string, BrushControlFunc>("Hexagon", x => new CoolHexagon(x) ),
           new KeyValuePair<string, BrushControlFunc>("Trapezoid", x => new CoolTrapezoid(x) ),
           new KeyValuePair<string, BrushControlFunc>("Star", x => new CoolStar(x) ),
           new KeyValuePair<string, BrushControlFunc>("Square", x => new CoolSquare(x) ),
           new KeyValuePair<string, BrushControlFunc>("Triangle", x => new CoolTriangle(x) ),
           new KeyValuePair<string, BrushControlFunc>("Heart", x => new CoolHeart(x) )
       };

      public static UserControl NewUserControlFrom(FigureTemplate template)
      {
         UserControl retVal = null;
         //We'll wait for Hardware Accelerated Shader Effects in SP1

         if (template.Letter.Length == 1 && Char.IsLetterOrDigit(template.Letter[0]))
         {
            retVal = new CoolLetter(template.Fill.Clone(), template.Letter);
         }
         else
         {
            retVal = template.GeneratorFunc(template.Fill.Clone());
         }

         Tweener.TransitionType randomTransition1 = (Tweener.TransitionType)Utils.RandomBetweenTwoNumbers(1, (int)Tweener.TransitionType.EaseOutInBounce);
         var ani1 = Tweener.Tween.CreateAnimation(randomTransition1, 0, 1, new TimeSpan(0, 0, 0, 1),30);
         Tweener.TransitionType randomTransition2 = (Tweener.TransitionType)Utils.RandomBetweenTwoNumbers(1, (int)Tweener.TransitionType.EaseOutInBounce);
         var ani2 = Tweener.Tween.CreateAnimation(randomTransition2, 360, 0, new TimeSpan(0, 0, 0, 1),30);
         retVal.RenderTransformOrigin = new Point(0.5, 0.5);
         var group = new TransformGroup();
         group.Children.Add(new ScaleTransform());
         group.Children.Add(new RotateTransform());
         retVal.RenderTransform = group;
         group.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, ani1);
         group.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, ani1);
         group.Children[1].BeginAnimation(RotateTransform.AngleProperty, ani2);

         //TODO: TOO SLOW! Waiting for ShaderEffects in 3.5SP1
         //if (Settings.Default.BitmapEffects)
         //{
         //   retVal.BitmapEffect = template.Effect.Clone();
         //}
         return retVal;
      }

      //TODO: Should this be in XAML? Would that make it better?
      //TODO: Should I change the height, width and stroke to be relative to the screen size?
      //TODO: Where can I get REALLY complex shapes like animal vectors or custom pics? Where do I store them?

      public static FigureTemplate GenerateFigureTemplate(string letter)
      {
         Color c = Utils.GetRandomColor();

         var nameFunc = hashTableOfFigureGenerators[Utils.RandomBetweenTwoNumbers(0, hashTableOfFigureGenerators.Count - 1)];
         return new FigureTemplate
         {
            Color = c,
            Name = (letter.Length == 1 && Char.IsLetterOrDigit(letter[0])) ? letter : nameFunc.Key,
            GeneratorFunc = nameFunc.Value,
            Fill = Utils.GetGradientBrush(c),
            Letter = letter,
            Effect = Animation.GetRandomBitmapEffect()
         };
      }
   }
}
