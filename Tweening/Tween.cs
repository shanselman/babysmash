namespace Tweener
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Provides the implementation of tween attached properties.
    /// </summary>
    public static class Tween
    {
        /// <summary>
        /// The default number of keyframes to generate per second.
        /// </summary>
        public const int DefaultFps = 20;

        /// <summary>
        /// Defines the tween transition type.
        /// </summary>
        public static readonly DependencyProperty TransitionTypeProperty =
            DependencyProperty.RegisterAttached("TransitionType", typeof(TransitionType), typeof(Tween), new PropertyMetadata(OnTweenChanged));

        /// <summary>
        /// Defines the source value of a tween double animation.
        /// </summary>
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.RegisterAttached("From", typeof(double), typeof(Tween), new PropertyMetadata(OnTweenChanged));

        /// <summary>
        /// Defines the target value of a tween double animation.
        /// </summary>
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.RegisterAttached("To", typeof(double), typeof(Tween), new PropertyMetadata(OnTweenChanged));

        /// <summary>
        /// The frames-per-second attached property.
        /// </summary>
        /// <remarks>The FPS attached property defines the number of keyframes to generate per second.</remarks>
        public static readonly DependencyProperty FpsProperty =
            DependencyProperty.RegisterAttached("Fps", typeof(double), typeof(Tween), new PropertyMetadata(OnTweenChanged));

        private delegate double Equation(params double[] args);

        private static void OnTweenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DoubleAnimationUsingKeyFrames animation = o as DoubleAnimationUsingKeyFrames;

            if (animation != null && animation.Duration.HasTimeSpan)
            {
                TransitionType type = GetTransitionType(animation);
                double from = GetFrom(animation);
                double to = GetTo(animation);
                double fps = GetFps(animation);

                if (fps <= 0)
                {
                    fps = DefaultFps;
                }

                FillAnimation(animation, type, from, to, animation.Duration.TimeSpan, fps);
            }
        }

        private static void FillAnimation(DoubleAnimationUsingKeyFrames animation, TransitionType type, double from, double to, TimeSpan duration, double fps)
        {
            Equation equation = (Equation)Delegate.CreateDelegate(typeof(Equation), typeof(Equations).GetMethod(type.ToString()));

            double total = duration.TotalMilliseconds;
            double step = 1000D / (double)fps;

            // clear animation
            animation.KeyFrames.Clear();

            for (double i = 0; i < total; i += step)
            {
                LinearDoubleKeyFrame frame = new LinearDoubleKeyFrame();
                frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(i));
                frame.Value = equation(i, from, to - from, total);

                animation.KeyFrames.Add(frame);
            }

            // always add exact final key frame
            LinearDoubleKeyFrame finalFrame = new LinearDoubleKeyFrame();
            finalFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(total));
            finalFrame.Value = to;
            animation.KeyFrames.Add(finalFrame);
        }

        /// <summary>
        /// Creates a tween animation with a default number of frames per second..
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        public static DoubleAnimationUsingKeyFrames CreateAnimation(TransitionType type, double from, double to, TimeSpan duration)
        {
            return CreateAnimation(type, from, to, duration, DefaultFps);
        }

        /// <summary>
        /// Creates a tween animation.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="fps">The number of keyframes to generate per second.</param>
        /// <returns></returns>
        public static DoubleAnimationUsingKeyFrames CreateAnimation(TransitionType type, double from, double to, TimeSpan duration, double fps)
        {
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            FillAnimation(animation, type, from, to, duration, fps);
            return animation;
        }

        /// <summary>
        /// Gets the tween transition type.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static TransitionType GetTransitionType(DependencyObject o)
        {
            return (TransitionType)o.GetValue(TransitionTypeProperty);
        }

        /// <summary>
        /// Sets the tween transition type.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="value">The value.</param>
        public static void SetTransitionType(DependencyObject o, TransitionType value)
        {
            o.SetValue(TransitionTypeProperty, value);
        }

        /// <summary>
        /// Gets the tween's starting value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static double GetFrom(DependencyObject o)
        {
            return (double)o.GetValue(FromProperty);
        }

        /// <summary>
        /// Sets the tween's starting value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="value">The value.</param>
        public static void SetFrom(DependencyObject o, double value)
        {
            o.SetValue(FromProperty, value);
        }

        /// <summary>
        /// Gets the tween's ending value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        public static double GetTo(DependencyObject o)
        {
            return (double)o.GetValue(ToProperty);
        }

        /// <summary>
        /// Sets the tween's ending value.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="value">The value.</param>
        public static void SetTo(DependencyObject o, double value)
        {
            o.SetValue(ToProperty, value);
        }

        /// <summary>
        /// Gets the number of keyframes to generate per second.
        /// </summary>
        /// <param name="o">The o.</param>
        public static double GetFps(DependencyObject o)
        {
            return (double)o.GetValue(FpsProperty);
        }

        /// <summary>
        /// Sets the number of keyframes to generate per second.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="value">The value.</param>
        public static void SetFps(DependencyObject o, double value)
        {
            o.SetValue(FpsProperty, value);
        }
    }
}