using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace BabySmash
{
   class HiResTextBlock : FrameworkElement
   {
      public HiResTextBlock()
         : base()
      {
         RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
         this.SnapsToDevicePixels = true;
      }

      Geometry m_textg;
      static Pen m_pen;

      protected override void OnRender(DrawingContext drawingContext)
      {
         drawingContext.DrawGeometry(Fill, m_pen, m_textg);
      }

      private static void OnTextInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         m_pen = new Pen(((HiResTextBlock)d).Stroke, ((HiResTextBlock)d).StrokeThickness);
         m_pen.LineJoin = PenLineJoin.Round;
         m_pen.MiterLimit = 1;
         m_pen = m_pen.GetAsFrozen() as Pen;
         ((HiResTextBlock)d).GenerateText();
      }

      private void GenerateText()
      {
         if (Font == null)
            Font = new FontFamily("Arial");

         FormattedText fText = new FormattedText(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(
                Font,
                FontStyles.Normal,
                FontWeights.Heavy,
                FontStretches.Normal),
            FontSize,
            Brushes.Black
            );

         m_textg = fText.BuildGeometry(new Point(0, 0)).GetAsFrozen() as Geometry;
      }

      #region DPs
      public Brush Stroke
      {
         get
         {
            return (Brush)GetValue(StrokeProperty);
         }

         set
         {
            SetValue(StrokeProperty, value);
         }
      }

      public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
          "Stroke",
          typeof(Brush),
          typeof(HiResTextBlock),
          new FrameworkPropertyMetadata(
               new SolidColorBrush(Colors.Black),
               FrameworkPropertyMetadataOptions.AffectsRender,
               new PropertyChangedCallback(OnTextInvalidated),
               null
               )
          );

      public ushort StrokeThickness
      {
         get
         {
            return (ushort)GetValue(StrokeThicknessProperty);
         }

         set
         {
            SetValue(StrokeThicknessProperty, value);
         }
      }

      public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
          "StrokeThickness",
          typeof(ushort),
          typeof(HiResTextBlock),
          new FrameworkPropertyMetadata(
               (ushort)1,
               FrameworkPropertyMetadataOptions.AffectsRender,
               new PropertyChangedCallback(OnTextInvalidated),
               null
               )
          );

      public string Text
      {
         get
         {
            return (string)GetValue(TextProperty);
         }

         set
         {
            SetValue(TextProperty, value);
         }
      }

      public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
          "Text",
          typeof(string),
          typeof(HiResTextBlock),
          new FrameworkPropertyMetadata(
               "",
               FrameworkPropertyMetadataOptions.AffectsRender,
               new PropertyChangedCallback(OnTextInvalidated),
               null
               )
          );

      public double FontSize
      {
         get
         {
            return (double)GetValue(FontSizeProperty);
         }

         set
         {
            SetValue(FontSizeProperty, value);
         }
      }

      public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
          "FontSize",
          typeof(double),
          typeof(HiResTextBlock),
          new FrameworkPropertyMetadata(
               (double)12,
               FrameworkPropertyMetadataOptions.AffectsRender,
               new PropertyChangedCallback(OnTextInvalidated),
               null
               )
          );

      public Brush Fill
      {
         get
         {
            return (Brush)GetValue(FillProperty);
         }

         set
         {
            SetValue(FillProperty, value);
         }
      }

      public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
          "Fill",
          typeof(Brush),
          typeof(HiResTextBlock),
          new FrameworkPropertyMetadata(
              new SolidColorBrush(Colors.White),
              FrameworkPropertyMetadataOptions.AffectsRender,
              new PropertyChangedCallback(OnTextInvalidated),
              null
              )
          );

      public FontFamily Font
      {
         get
         {
            return (FontFamily)GetValue(FontProperty);
         }

         set
         {
            SetValue(FontProperty, value);
         }
      }

      public static readonly DependencyProperty FontProperty = DependencyProperty.Register(
           "Font",
           typeof(FontFamily),
           typeof(HiResTextBlock),
           new FrameworkPropertyMetadata(
               new FontFamily("Arial"),
               FrameworkPropertyMetadataOptions.AffectsRender,
               new PropertyChangedCallback(OnTextInvalidated),
               null
               )
           );

      #endregion
   }
}
