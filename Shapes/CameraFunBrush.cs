using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace BabySmash
{
    /// <summary>
    /// Turns a webcam snapshot into a silly fill for a shape: a random funny prop
    /// (googly eyes, clown nose, mustache, ...), sparkles, a glow in the shape's color
    /// (so the spoken "Red Circle" still matches) and sometimes a playful distortion.
    /// Props assume the face is roughly centered, which is typical for a webcam.
    /// </summary>
    internal static class CameraFunBrush
    {
        // Prop artwork is designed in a 100x100 box and placed with PlaceGeometry.
        private const string MustacheData = "M50,42 C42,28 18,26 4,44 C2,47 6,49 9,47 C18,42 26,50 32,56 C40,63 48,58 50,50 C52,58 60,63 68,56 C74,50 82,42 91,47 C94,49 98,47 96,44 C82,26 58,28 50,42 Z";
        private const string PartyHatData = "M50,0 L82,100 L18,100 Z";
        private const string CrownData = "M8,92 L8,30 L30,58 L50,12 L70,58 L92,30 L92,92 Z";
        private const string SparkleData = "M50,0 C54,38 62,46 100,50 C62,54 54,62 50,100 C46,62 38,54 0,50 C38,46 46,38 50,0 Z";

        private static readonly Color[] PartyColors =
        {
            Colors.DeepPink, Colors.Gold, Colors.DeepSkyBlue, Colors.LimeGreen, Colors.MediumPurple, Colors.Orange
        };

        private static Random Random => System.Random.Shared;

        public static Brush Create(BitmapSource frame, Color shapeColor)
        {
            double w = frame.Width;
            double h = frame.Height;
            var bounds = new Rect(0, 0, w, h);

            var drawing = new DrawingGroup();
            drawing.Children.Add(new ImageDrawing(frame, bounds));
            drawing.Children.Add(CreateColorGlow(bounds, shapeColor));

            switch (Utils.RandomBetweenTwoNumbers(0, 4))
            {
                case 0: AddGooglyEyes(drawing, w, h); break;
                case 1: AddClownNose(drawing, w, h); break;
                case 2: AddMustache(drawing, w, h); break;
                case 3: AddSunglasses(drawing, w, h); break;
                default: AddCatWhiskers(drawing, w, h); break;
            }

            switch (Utils.RandomBetweenTwoNumbers(0, 3))
            {
                case 0: AddPartyHat(drawing, w, h); break;
                case 1: AddCrown(drawing, w, h); break;
                default: break; // No hat half the time
            }

            if (Random.Next(2) == 0)
            {
                AddRosyCheeks(drawing, w, h);
            }

            AddSparkles(drawing, w, h, Utils.RandomBetweenTwoNumbers(3, 6));

            // Clip props that poke out of the photo so they don't change the brush bounds.
            drawing.ClipGeometry = new RectangleGeometry(bounds);

            var brush = new DrawingBrush(drawing) { Stretch = Stretch.UniformToFill };
            ApplyDistortion(brush);
            brush.Freeze();
            return brush;
        }

        private static void ApplyDistortion(DrawingBrush brush)
        {
            switch (Utils.RandomBetweenTwoNumbers(0, 5))
            {
                case 0: // Big head: zoomed in on the face
                    brush.Viewbox = new Rect(0.2, 0.1, 0.6, 0.75);
                    break;
                case 1: // Chubby cheeks: gently stretched wide
                    brush.Viewbox = new Rect(0.15, 0, 0.7, 1);
                    brush.Stretch = Stretch.Fill;
                    break;
                case 2: // Mirror
                    brush.RelativeTransform = new ScaleTransform(-1, 1, 0.5, 0.5);
                    break;
                case 3: // Upside down
                    brush.RelativeTransform = new ScaleTransform(1, -1, 0.5, 0.5);
                    break;
                default: // Plain photo, props do the talking
                    break;
            }
        }

        private static Drawing CreateColorGlow(Rect bounds, Color color)
        {
            var glow = new RadialGradientBrush();
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 0.0));
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 0.55));
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(0xC0, color.R, color.G, color.B), 1.0));
            return new GeometryDrawing(glow, null, new RectangleGeometry(bounds));
        }

        private static void AddGooglyEyes(DrawingGroup drawing, double w, double h)
        {
            double radius = w * 0.075;
            var outline = new Pen(Brushes.Black, w * 0.008);
            foreach (double x in new[] { 0.40, 0.60 })
            {
                var center = new Point(w * x, h * 0.42);
                drawing.Children.Add(new GeometryDrawing(Brushes.White, outline, new EllipseGeometry(center, radius, radius)));

                // Each pupil wanders a bit for that googly look.
                double angle = Random.NextDouble() * Math.PI * 2;
                var pupil = new Point(
                    center.X + Math.Cos(angle) * radius * 0.4,
                    center.Y + Math.Sin(angle) * radius * 0.4);
                drawing.Children.Add(new GeometryDrawing(Brushes.Black, null, new EllipseGeometry(pupil, radius * 0.5, radius * 0.5)));
                drawing.Children.Add(new GeometryDrawing(Brushes.White, null,
                    new EllipseGeometry(new Point(pupil.X - radius * 0.18, pupil.Y - radius * 0.18), radius * 0.13, radius * 0.13)));
            }
        }

        private static void AddClownNose(DrawingGroup drawing, double w, double h)
        {
            double radius = w * 0.06;
            var center = new Point(w * 0.5, h * 0.55);
            var nose = new RadialGradientBrush(Color.FromRgb(0xFF, 0x60, 0x60), Color.FromRgb(0xC0, 0x00, 0x00))
            {
                GradientOrigin = new Point(0.35, 0.3)
            };
            drawing.Children.Add(new GeometryDrawing(nose, new Pen(Brushes.DarkRed, w * 0.005), new EllipseGeometry(center, radius, radius)));
            drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromArgb(0xC0, 0xFF, 0xFF, 0xFF)), null,
                new EllipseGeometry(new Point(center.X - radius * 0.35, center.Y - radius * 0.35), radius * 0.22, radius * 0.15)));
        }

        private static void AddMustache(DrawingGroup drawing, double w, double h)
        {
            Brush color = Random.Next(3) == 0 ? new SolidColorBrush(PartyColors[Random.Next(PartyColors.Length)]) : Brushes.SaddleBrown;
            drawing.Children.Add(new GeometryDrawing(color, new Pen(Brushes.Black, w * 0.004),
                PlaceGeometry(MustacheData, new Point(w * 0.5, h * 0.64), w * 0.3)));
        }

        private static void AddSunglasses(DrawingGroup drawing, double w, double h)
        {
            var frameColor = new SolidColorBrush(PartyColors[Random.Next(PartyColors.Length)]);
            var framePen = new Pen(frameColor, w * 0.014);
            var lens = new SolidColorBrush(Color.FromArgb(0xE6, 0x10, 0x10, 0x20));
            double lensWidth = w * 0.16;
            double lensHeight = h * 0.12;
            double top = h * 0.42 - lensHeight / 2;

            foreach (double x in new[] { 0.40, 0.60 })
            {
                var rect = new Rect(w * x - lensWidth / 2, top, lensWidth, lensHeight);
                drawing.Children.Add(new GeometryDrawing(lens, framePen, new RectangleGeometry(rect, lensWidth * 0.25, lensHeight * 0.3)));
                // Shiny glare streak
                drawing.Children.Add(new GeometryDrawing(null, new Pen(new SolidColorBrush(Color.FromArgb(0xB0, 0xFF, 0xFF, 0xFF)), w * 0.008),
                    new LineGeometry(new Point(rect.Left + lensWidth * 0.2, rect.Bottom - lensHeight * 0.3), new Point(rect.Left + lensWidth * 0.45, rect.Top + lensHeight * 0.25))));
            }

            drawing.Children.Add(new GeometryDrawing(null, framePen,
                new LineGeometry(new Point(w * 0.48, h * 0.41), new Point(w * 0.52, h * 0.41))));
        }

        private static void AddCatWhiskers(DrawingGroup drawing, double w, double h)
        {
            var pen = new Pen(Brushes.Black, w * 0.007) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            double noseY = h * 0.56;
            foreach (int side in new[] { -1, 1 })
            {
                for (int i = -1; i <= 1; i++)
                {
                    var start = new Point(w * (0.5 + side * 0.06), noseY + h * 0.035 + i * h * 0.03);
                    var end = new Point(w * (0.5 + side * 0.26), noseY + i * h * 0.07);
                    drawing.Children.Add(new GeometryDrawing(null, pen, new LineGeometry(start, end)));
                }
            }

            var nose = new PathFigure { StartPoint = new Point(w * 0.47, noseY), IsClosed = true };
            nose.Segments.Add(new LineSegment(new Point(w * 0.53, noseY), true));
            nose.Segments.Add(new LineSegment(new Point(w * 0.5, noseY + h * 0.045), true));
            drawing.Children.Add(new GeometryDrawing(Brushes.HotPink, new Pen(Brushes.Black, w * 0.004), new PathGeometry(new[] { nose })));
        }

        private static void AddRosyCheeks(DrawingGroup drawing, double w, double h)
        {
            var blush = new RadialGradientBrush(Color.FromArgb(0x90, 0xFF, 0x50, 0x80), Color.FromArgb(0x00, 0xFF, 0x50, 0x80));
            foreach (double x in new[] { 0.34, 0.66 })
            {
                drawing.Children.Add(new GeometryDrawing(blush, null, new EllipseGeometry(new Point(w * x, h * 0.6), w * 0.07, h * 0.06)));
            }
        }

        private static void AddPartyHat(DrawingGroup drawing, double w, double h)
        {
            Color first = PartyColors[Random.Next(PartyColors.Length)];
            Color second = PartyColors[Random.Next(PartyColors.Length)];
            var stripes = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0.25, 0.25),
                SpreadMethod = GradientSpreadMethod.Repeat
            };
            stripes.GradientStops.Add(new GradientStop(first, 0.0));
            stripes.GradientStops.Add(new GradientStop(first, 0.5));
            stripes.GradientStops.Add(new GradientStop(second, 0.5));
            stripes.GradientStops.Add(new GradientStop(second, 1.0));

            double size = w * 0.26;
            var center = new Point(w * 0.5, h * 0.13);
            drawing.Children.Add(new GeometryDrawing(stripes, new Pen(Brushes.Black, w * 0.005), PlaceGeometry(PartyHatData, center, size)));

            var pomPom = new Point(center.X, center.Y - size / 2);
            drawing.Children.Add(new GeometryDrawing(Brushes.White, new Pen(Brushes.Black, w * 0.004),
                new EllipseGeometry(pomPom, w * 0.025, w * 0.025)));
        }

        private static void AddCrown(DrawingGroup drawing, double w, double h)
        {
            var gold = new LinearGradientBrush(Colors.Gold, Colors.DarkGoldenrod, 90);
            double size = w * 0.3;
            var center = new Point(w * 0.5, h * 0.12);
            drawing.Children.Add(new GeometryDrawing(gold, new Pen(Brushes.SaddleBrown, w * 0.005), PlaceGeometry(CrownData, center, size)));

            // Jewels along the band
            Brush[] jewels = { Brushes.Red, Brushes.DeepSkyBlue, Brushes.LimeGreen };
            for (int i = 0; i < jewels.Length; i++)
            {
                var jewel = new Point(center.X + (i - 1) * size * 0.28, center.Y + size * 0.27);
                drawing.Children.Add(new GeometryDrawing(jewels[i], null, new EllipseGeometry(jewel, w * 0.015, w * 0.015)));
            }
        }

        private static void AddSparkles(DrawingGroup drawing, double w, double h, int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Keep sparkles near the edges so they don't cover the face.
                double x = Random.NextDouble();
                double y = Random.NextDouble();
                if (Math.Abs(x - 0.5) < 0.3 && Math.Abs(y - 0.5) < 0.3)
                {
                    x = x < 0.5 ? x * 0.4 : 0.6 + (x - 0.5) * 0.8;
                }

                Color color = Random.Next(3) == 0 ? Colors.White : PartyColors[Random.Next(PartyColors.Length)];
                double size = w * (0.06 + Random.NextDouble() * 0.08);
                drawing.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromArgb(0xE6, color.R, color.G, color.B)),
                    new Pen(new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)), w * 0.003),
                    PlaceGeometry(SparkleData, new Point(w * x, h * y), size)));
            }
        }

        /// <summary>Scales artwork drawn in a 100x100 box to <paramref name="size"/> and centers it on <paramref name="center"/>.</summary>
        private static Geometry PlaceGeometry(string data, Point center, double size)
        {
            Geometry geometry = Geometry.Parse(data).Clone();
            double scale = size / 100.0;
            geometry.Transform = new MatrixTransform(scale, 0, 0, scale, center.X - size / 2, center.Y - size / 2);
            return geometry;
        }
    }
}
