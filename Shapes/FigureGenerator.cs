using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using BrushControlFunc = System.Func<System.Windows.Media.Brush, System.Windows.Controls.UserControl>;
using BabySmash.Properties;

namespace BabySmash
{
    public class FigureTemplate
    {
        public Brush Fill { get; set; }
        public Color Color { get; set; }
        public BrushControlFunc GeneratorFunc { get; set; }
        public Effect Effect { get; set; }
        public string Name { get; set; }
        public string Letter { get; set; }
    }

    public class FigureGenerator
    {
        private static readonly List<KeyValuePair<string, BrushControlFunc>> hashTableOfFigureGenerators = new List<KeyValuePair<string, BrushControlFunc>>
       {
           new KeyValuePair<string, BrushControlFunc>("Circle", x => new CoolCircle(x) ),
           new KeyValuePair<string, BrushControlFunc>("Oval", x => new CoolOval(x) ),
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
            
            if (template.Letter.Length == 1 && Char.IsLetterOrDigit(template.Letter[0]))
            {
                retVal = new CoolLetter(template.Fill.Clone(), template.Letter[0]);
            }
            else
            {
                retVal = template.GeneratorFunc(template.Fill.Clone());
            }

            var randomTransition1 = (Tweener.TransitionType)Utils.RandomBetweenTwoNumbers(1, (int)Tweener.TransitionType.EaseOutInBounce);
            var ani1 = Tweener.Tween.CreateAnimation(randomTransition1, 0, 1, new TimeSpan(0, 0, 0, 1), 30);
            var randomTransition2 = (Tweener.TransitionType)Utils.RandomBetweenTwoNumbers(1, (int)Tweener.TransitionType.EaseOutInBounce);
            var ani2 = Tweener.Tween.CreateAnimation(randomTransition2, 360, 0, new TimeSpan(0, 0, 0, 1), 30);
            retVal.RenderTransformOrigin = new Point(0.5, 0.5);
            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform());
            group.Children.Add(new RotateTransform());
            retVal.RenderTransform = group;
            group.Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, ani1);
            group.Children[0].BeginAnimation(ScaleTransform.ScaleYProperty, ani1);
            group.Children[1].BeginAnimation(RotateTransform.AngleProperty, ani2);

            if (Settings.Default.UseEffects)
            {
                retVal.Effect = template.Effect.Clone();
            }
            
            return retVal;
        }

        //TODO: Should this be in XAML? Would that make it better?
        //TODO: Should I change the height, width and stroke to be relative to the screen size?
        //TODO: Where can I get REALLY complex shapes like animal vectors or custom pics? Where do I store them?

        public static FigureTemplate GenerateFigureTemplate(char displayChar)
        {
            Color c = Utils.GetRandomColor();

            var nameFunc = hashTableOfFigureGenerators[Utils.RandomBetweenTwoNumbers(0, hashTableOfFigureGenerators.Count - 1)];
            return new FigureTemplate
            {
                Color = c,
                Name = Char.IsLetterOrDigit(displayChar) ? displayChar.ToString() : nameFunc.Key,
                GeneratorFunc = nameFunc.Value,
                Fill = Utils.GetGradientBrush(c),
                Letter = displayChar.ToString(),
                Effect = Animation.GetRandomBitmapEffect()
            };
        }
    }
}