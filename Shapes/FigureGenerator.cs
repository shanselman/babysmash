using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BabySmash;

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

            //We'll wait for Hardware Accelerated Shader Effects in SP1
            if (Properties.Settings.Default.BitmapEffects)
                f.Shape.BitmapEffect = GetRandomBitmapEffect();

            figures.Add(f);
            if (Properties.Settings.Default.FadeAway) s.Begin(container);
        }

        private BitmapEffect GetRandomBitmapEffect()
        {
            int e = Utils.RandomBetweenTwoNumbers(0, 3);
            switch (e)
            {
                case 0:
                    return new BevelBitmapEffect();
                case 1:
                    return new DropShadowBitmapEffect();
                case 2:
                    return new EmbossBitmapEffect();
                case 3:
                    return new OuterGlowBitmapEffect();
            }
            return new BevelBitmapEffect();
        }

        private static Storyboard CreateStoryboardAnimation(FrameworkElement container, UIElement shape, DependencyProperty dp)
        {
            var st = new Storyboard();
            NameScope.SetNameScope(container, new NameScope());
            container.RegisterName("shape", shape);

            var d = new DoubleAnimation();
            d.From = 1.0;
            d.To = 0.0;
            d.Duration = new Duration(TimeSpan.FromSeconds(7));
            d.AutoReverse = false;

            st.Children.Add(d);
            Storyboard.SetTargetName(d, "shape");
            Storyboard.SetTargetProperty(d, new PropertyPath(dp));
            return st;
        }

        //TODO: Should this be in XAML? Would that make it better?
        //TODO: Should I change the height, width and stroke to be relative to the screen size?
        //TODO: Where can I get REALLY complex shapes like animal vectors or custom pics? Where do I store them?

        private static readonly List<Func<Brush, Figure>> listOfPotentialFigures = new List<Func<Brush, Figure>>
        {
            x => new SquareFigure(x),
            x => new CircleFigure(x),
            x => new TriangleFigure(x),
            x => new StarFigure(x),
            x => new HeartFigure(x),
            x => new TrapezoidFigure(x),
            x => new RectangleFigure(x)
        };

        private static Figure GenerateFigure(string letter)
        {
            var fill = Utils.GetRandomColoredBrush();
            if (letter.Length == 1 && Char.IsLetterOrDigit(letter[0]))
                return new LetterFigure(fill, letter);
            var myFunc = listOfPotentialFigures[
                Utils.RandomBetweenTwoNumbers(0, listOfPotentialFigures.Count - 1)];
            return myFunc(fill);
        }
    }
}

