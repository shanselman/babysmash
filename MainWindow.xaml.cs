using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace BabySmash
{
    public partial class MainWindow : Window
    {
        private readonly Controller controller;
        public Controller Controller { get { return controller; } }
        private const int VkShift = 0x10;
        private const int VkControl = 0x11;
        private const int VkMenu = 0x12;
        private const int VkLeftWindows = 0x5B;
        private const int VkRightWindows = 0x5C;
        private const int VkO = 0x4F;
        private readonly DispatcherTimer optionsGestureTimer;
        private readonly Stopwatch optionsGestureStopwatch = new();
        private readonly HashSet<Key> pressedKeys = new();
        private bool optionsGestureRequiresRelease;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int virtualKey);

        private UserControl customCursor;
        public UserControl CustomCursor { get { return customCursor; } set { customCursor = value; } }

        // FPS counter
        private int _frameCount;
        private readonly Stopwatch _fpsStopwatch = new();
        private bool _showFps;

        public void AddFigure(UserControl c)
        {
            this.figuresCanvas.Children.Add(c);
        }

        public void RemoveFigure(UserControl c)
        {
            this.figuresCanvas.Children.Remove(c);
        }

        public MainWindow(Controller c, bool showFps = false)
        {
            this.controller = c;
            this.DataContext = controller;
            _showFps = showFps;
            InitializeComponent();

            optionsGestureTimer = new DispatcherTimer(DispatcherPriority.Input)
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            optionsGestureTimer.Tick += OptionsGestureTimer_Tick;

            // Initialize cursor early to prevent NullReferenceException in mouse events (OnMouseEnter, OnMouseLeave, OnMouseMove)
            AssertCursor();

            if (_showFps)
            {
                fpsLabel.Visibility = Visibility.Visible;
                CompositionTarget.Rendering += OnRendering;
                _fpsStopwatch.Start();
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            _frameCount++;
            if (_fpsStopwatch.ElapsedMilliseconds >= 1000)
            {
                var fps = _frameCount * 1000.0 / _fpsStopwatch.ElapsedMilliseconds;
                fpsLabel.Text = $"FPS: {fps:F0} | Shapes: {figuresCanvas.Children.Count}";
                _frameCount = 0;
                _fpsStopwatch.Restart();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ResetOptionsGesture();
            optionsGestureTimer.Tick -= OptionsGestureTimer_Tick;

            if (_showFps)
            {
                CompositionTarget.Rendering -= OnRendering;
            }
            base.OnClosed(e);
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

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            pressedKeys.Remove(key);
            if (IsOptionsGestureKey(key) &&
                (optionsGestureStopwatch.IsRunning || optionsGestureRequiresRelease))
            {
                if (IsOptionsGestureReleased())
                {
                    ResetOptionsGesture();
                }

                e.Handled = true;
                return;
            }

            e.Handled = true;
            controller.ProcessKey(this, e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            pressedKeys.Add(key);
            if (optionsGestureRequiresRelease)
            {
                if (IsOptionsGestureKey(key))
                {
                    e.Handled = true;
                }

                return;
            }

            if (key == Key.O && IsExactOptionsGestureHeld())
            {
                if (!optionsGestureStopwatch.IsRunning)
                {
                    optionsGestureStopwatch.Restart();
                    optionsGestureTimer.Start();
                }

                e.Handled = true;
                return;
            }

            if (optionsGestureStopwatch.IsRunning)
            {
                CancelOptionsGestureUntilRelease();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            ResetOptionsGestureAfterDialog();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            ResetOptionsGesture();
            optionsGestureRequiresRelease = true;
            pressedKeys.Clear();
            base.OnDeactivated(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (IsLoaded && !controller.isOptionsDialogShown)
            {
                Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action(() => controller.RestoreKioskState()));
            }
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

        internal void RestoreCursor()
        {
            AssertCursor();
            Cursor = Cursors.None;

            if (CustomCursor == null)
            {
                return;
            }

            Point position = Mouse.GetPosition(mouseDragCanvas);
            Canvas.SetTop(CustomCursor, position.Y);
            Canvas.SetLeft(CustomCursor, position.X);
            Canvas.SetZIndex(CustomCursor, int.MaxValue);
            CustomCursor.Visibility = IsMouseOver ? Visibility.Visible : Visibility.Hidden;
        }

        private void OptionsGestureTimer_Tick(object sender, EventArgs e)
        {
            if (optionsGestureRequiresRelease)
            {
                if (IsOptionsGestureReleased())
                {
                    ResetOptionsGesture();
                }

                return;
            }

            if (!optionsGestureStopwatch.IsRunning)
            {
                optionsGestureTimer.Stop();
                return;
            }

            if (!IsExactOptionsGestureHeld())
            {
                CancelOptionsGestureUntilRelease();
                return;
            }

            if (optionsGestureStopwatch.Elapsed >= TimeSpan.FromSeconds(3))
            {
                optionsGestureStopwatch.Reset();
                optionsGestureTimer.Stop();
                optionsGestureRequiresRelease = true;
                controller.ShowOptionsDialog();
            }
        }

        internal void ResetOptionsGestureAfterDialog()
        {
            ResetOptionsGesture();
            optionsGestureRequiresRelease = !IsOptionsGestureReleased();
            if (optionsGestureRequiresRelease)
            {
                optionsGestureTimer.Start();
            }
        }

        internal void CancelOptionsGestureFromHook()
        {
            CancelOptionsGestureUntilRelease();
        }

        private void CancelOptionsGestureUntilRelease()
        {
            optionsGestureStopwatch.Reset();
            optionsGestureRequiresRelease = true;
            optionsGestureTimer.Start();
        }

        private void ResetOptionsGesture()
        {
            optionsGestureStopwatch.Reset();
            optionsGestureTimer.Stop();
            optionsGestureRequiresRelease = false;
        }

        private static bool IsOptionsGestureKey(Key key)
        {
            return key == Key.O ||
                   key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift;
        }

        private bool IsExactOptionsGestureHeld()
        {
            return IsKeyDown(VkO) &&
                   IsKeyDown(VkControl) &&
                   IsKeyDown(VkMenu) &&
                   IsKeyDown(VkShift) &&
                   !IsKeyDown(VkLeftWindows) &&
                   !IsKeyDown(VkRightWindows) &&
                   pressedKeys.All(IsOptionsGestureKey);
        }

        private static bool IsOptionsGestureReleased()
        {
            return !IsKeyDown(VkO) &&
                   !IsKeyDown(VkControl) &&
                   !IsKeyDown(VkMenu) &&
                   !IsKeyDown(VkShift);
        }

        private static bool IsKeyDown(int virtualKey)
        {
            return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        }
    }
}