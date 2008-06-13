using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace BabySmash
{
    public class Star : Shape
    {
        // Using a DependencyProperty as the backing store for NumberOfPoints.     
        public static readonly DependencyProperty NumberOfPointsProperty =
            DependencyProperty.Register("NumberOfPoints", typeof(int), typeof(Shape), new UIPropertyMetadata(5));

        public int NumberOfPoints
        {
            get { return (int)GetValue(NumberOfPointsProperty); }
            set { SetValue(NumberOfPointsProperty, value); }
        }
        
        protected override Geometry DefiningGeometry
        {
            get { return CreateStarGeometry(NumberOfPoints); }
        }

        public Geometry CreateStarGeometry(int numberOfPoints)
        {
           const double outerRadius = 300;
           const double innerRadius = 90;
           Point[] points = new Point[numberOfPoints * 2 - 1];
           for (int i = 0; i < numberOfPoints * 2 - 1; ++i)
           {
              double radius = ((i & 1) == 0) ? innerRadius : outerRadius;
              double angle = Math.PI * (i + 1) / numberOfPoints;
              points[i] = new Point(radius * Math.Sin(angle), -radius * Math.Cos(angle));
           }
           PolyLineSegment segment = new PolyLineSegment(points, true);
           PathFigure starFigure = new PathFigure(new Point(0, -outerRadius), new PathSegment[] { segment }, true);
           return new PathGeometry(new PathFigure[] { starFigure });
        }
    }
}
