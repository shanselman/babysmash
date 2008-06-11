using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace BabySmash
{
   class ShapeFactory
   {
      public static Shape GetRandomShape(FrameworkElement fe)
      {
         //TODO: Should this be in XAML? Would that make it better?
         Shape retVal = null;
         Brush fill = Utils.GetRandomColoredBrush();
         int shape = Convert.ToInt32(Utils.RandomBetweenTwoNumbers(0, 6));
         //TODO: Should I change the height, width and stroke to be relative to the screen size?
         //TODO: I think I need a shapefactory?
         switch (shape)
         {
            case 0:
               retVal = new Rectangle()
               {
                  Name = "Square",
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
                  Name = "Circle",
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
                  Name = "Triangle",
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
                  Name = "Star",
                  NumberOfPoints = 5,
                  Height = 400,
                  Width = 400,
                  Fill = fill,
                  StrokeThickness = 5,
                  Stroke = Brushes.Black,
               };
               break;
            case 4:
               {
                  Path r = new Path();
                  r.Name = "Heart";
                  r.Data = (fe.FindResource("Heart") as Path).Data;
                  r.Fill = fill;
                  r.Stroke = Brushes.Black;
                  r.StrokeThickness = 5;
                  retVal = r;
                  break;
               }
            case 5:
               {
                  Path r = new Path();
                  r.Name = "Trapezoid";
                  r.Data = (fe.FindResource("Trapezoid") as Path).Data;
                  r.Fill = fill;
                  r.Stroke = Brushes.Black;
                  r.StrokeThickness = 5;
                  retVal = r;
                  break;
               }
            case 6:
               retVal = new Rectangle()
               {
                  Name = "Rectangle",
                  Fill = fill,
                  Height = 160,
                  Width = 380,
                  StrokeThickness = 5,
                  Stroke = Brushes.Black,
               };

               break;


         }
         return retVal;
      }

   }
}
