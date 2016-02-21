using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BabySmash
{
    public partial class MainWindow : Window
    {
        private readonly Controller controller;
        public Controller Controller { get { return controller; } }

        private UserControl customCursor;
        public UserControl CustomCursor { get { return customCursor; } set { customCursor = value; } }

        public void AddFigure(UserControl c)
        {
            this.figuresCanvas.Children.Add(c);
        }

        public void RemoveFigure(UserControl c)
        {
            this.figuresCanvas.Children.Remove(c);
        }

        public MainWindow(Controller c)
        {
            this.controller = c;
            this.DataContext = controller;
            InitializeComponent();

            //ResetCanvas();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            controller.MouseWheel(this, e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            controller.MouseUp(this, e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            controller.MouseDown(this, e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            AssertCursor();
            CustomCursor.Visibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            CustomCursor.Visibility = Visibility.Hidden;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (controller.isOptionsDialogShown == false)
            {
                CustomCursor.Visibility = Visibility.Visible;
                Point p = e.GetPosition(mouseDragCanvas);
                double pX = p.X;
                double pY = p.Y;
                Cursor = Cursors.None;
                Canvas.SetTop(CustomCursor, pY);
                Canvas.SetLeft(CustomCursor, pX);
                Canvas.SetZIndex(CustomCursor, int.MaxValue);
            }
            controller.MouseMove(this, e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            controller.ProcessKey(this, e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            controller.LostMouseCapture(this, e);
        }

        internal void AssertCursor()
        {
            try
            {
                mouseCursorCanvas.Children.Clear();
                CustomCursor = Utils.GetCursor();
                if (CustomCursor.Parent != null)
                {
                    ((Canvas)CustomCursor.Parent).Children.Remove(CustomCursor);
                }
                CustomCursor.RenderTransform = new ScaleTransform(0.5, 0.5);
                CustomCursor.Name = "customCursor";
                mouseCursorCanvas.Children.Insert(0, CustomCursor); //in front!
                CustomCursor.Visibility = Visibility.Hidden;
            }
            catch (System.NotSupportedException)
            {
                //we can die here if we ALT-F4 while in the Options Dialog
            }
        }

        private void Properties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            controller.ShowOptionsDialog();
        }
    }
}