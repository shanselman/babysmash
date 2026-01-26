using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using BabySmash.Core.Interfaces;
using BabySmash.Core.Models;
using BabySmash.Core.Services;
using BabySmash.Linux.Shapes;
using Microsoft.Extensions.DependencyInjection;

namespace BabySmash.Linux;

public partial class MainWindow : Window
{
    private readonly ITtsService _ttsService;
    private readonly IAudioService _audioService;
    private readonly ISettingsService _settingsService;
    private readonly WordFinder _wordFinder;
    private readonly Queue<Control> _figuresQueue = new();
    private readonly Queue<Shape> _mouseEllipsesQueue = new();
    private readonly List<Control> _figuresList = new();
    private readonly List<DispatcherTimer> _animationTimers = new();
    private readonly DispatcherTimer _focusTimer;
    private bool _isDrawing;
    private bool _isOptionsDialogShown;
    private Control? _customCursor;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get services from DI
        _ttsService = App.Services.GetRequiredService<ITtsService>();
        _audioService = App.Services.GetRequiredService<IAudioService>();
        _settingsService = App.Services.GetRequiredService<ISettingsService>();
        _wordFinder = App.Services.GetRequiredService<WordFinder>();
        
        // Hook up input events
        KeyDown += OnKeyDown;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
        
        // Focus management timer
        _focusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _focusTimer.Tick += (s, e) =>
        {
            if (!_isOptionsDialogShown)
            {
                Activate();
                Focus();
            }
        };
        
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Focus();
        _focusTimer.Start();
        
        // Play startup sound
        _audioService.PlaySound("EditedJackPlaysBabySmash.wav");
        
        // Setup custom cursor
        SetupCustomCursor();
    }

    private void SetupCustomCursor()
    {
        // Hide system cursor
        Cursor = new Cursor(StandardCursorType.None);
        
        // Create custom cursor (arrow)
        _customCursor = new FunCursor1();
        _customCursor.ZIndex = 10000; // Always on top
        _customCursor.IsHitTestVisible = false;
        _customCursor.RenderTransform = new ScaleTransform(0.5, 0.5); // Make it smaller
        
        var canvas = this.FindControl<Canvas>("mainCanvas");
        canvas?.Children.Add(_customCursor);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Hide info label on first keypress
        var infoLabel = this.FindControl<StackPanel>("infoLabel");
        if (infoLabel != null && infoLabel.IsVisible)
        {
            infoLabel.IsVisible = false;
        }
        
        // Exit on Escape
        if (e.Key == Key.Escape)
        {
            _focusTimer.Stop();
            CloseAllWindows();
            return;
        }

        // Options dialog on Alt+O
        if (e.Key == Key.O && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            ShowOptionsDialog();
            e.Handled = true;
            return;
        }

        // Get the display character
        char? displayChar = GetDisplayChar(e.Key);

        if (displayChar.HasValue)
        {
            // Add figure to all windows
            foreach (var window in App.Windows)
            {
                window.AddFigure(displayChar.Value);
            }
        }

        e.Handled = true;
    }

    private void CloseAllWindows()
    {
        foreach (var window in App.Windows)
        {
            if (window != this)
            {
                window.StopAllTimers();
                window.Close();
            }
        }
        StopAllTimers();
        Close();
    }

    private void StopAllTimers()
    {
        _focusTimer.Stop();
        foreach (var timer in _animationTimers.ToArray())
        {
            timer.Stop();
        }
        _animationTimers.Clear();
    }

    private DispatcherTimer CreateAnimationTimer(TimeSpan interval)
    {
        var timer = new DispatcherTimer { Interval = interval };
        _animationTimers.Add(timer);
        return timer;
    }

    private void RemoveAnimationTimer(DispatcherTimer timer)
    {
        timer.Stop();
        _animationTimers.Remove(timer);
    }

    private char? GetDisplayChar(Key key)
    {
        // Letters A-Z
        if (key >= Key.A && key <= Key.Z)
        {
            return (char)('A' + (key - Key.A));
        }
        
        // Numbers 0-9 (top row)
        if (key >= Key.D0 && key <= Key.D9)
        {
            return (char)('0' + (key - Key.D0));
        }
        
        // Numpad 0-9
        if (key >= Key.NumPad0 && key <= Key.NumPad9)
        {
            return (char)('0' + (key - Key.NumPad0));
        }
        
        // Any other key generates a random shape (matching Windows behavior)
        return '*';
    }

    private void AddFigure(char c)
    {
        // Apply ForceUppercase setting
        var forceUpper = _settingsService.Get(SettingsKeys.ForceUppercase, SettingsDefaults.ForceUppercase);
        if (forceUpper && char.IsLetter(c))
        {
            c = char.ToUpper(c);
        }
        
        var template = BabySmashUtils.GenerateFigureTemplate(c);
        var figure = FigureGenerator.CreateFigure(template);
        
        // Set face visibility based on settings
        var showFaces = _settingsService.Get(SettingsKeys.FacesOnShapes, SettingsDefaults.FacesOnShapes);
        FigureGenerator.SetFaceVisibility(figure, showFaces);
        
        // Calculate size and position
        double figureWidth = figure.Width > 0 ? figure.Width : 200;
        double figureHeight = figure.Height > 0 ? figure.Height : 200;
        
        var x = BabySmashUtils.RandomBetweenTwoNumbers(0, (int)Math.Max(1, Bounds.Width - figureWidth));
        var y = BabySmashUtils.RandomBetweenTwoNumbers(0, (int)Math.Max(1, Bounds.Height - figureHeight));
        
        Canvas.SetLeft(figure, x);
        Canvas.SetTop(figure, y);
        
        // Add spawn animation (scale + rotate)
        ApplySpawnAnimation(figure);
        
        figuresCanvas.Children.Add(figure);
        _figuresQueue.Enqueue(figure);
        _figuresList.Add(figure);

        // Check for word completion
        var word = _wordFinder.AddLetter(c);
        if (word != null)
        {
            _ttsService.Speak($"You spelled {word}!");
            AnimateLettersIntoWord(word);
        }
        else
        {
            PlaySound(template);
        }

        // Clear old figures if queue is too long
        var clearAfter = _settingsService.Get(SettingsKeys.ClearAfter, SettingsDefaults.ClearAfter);
        while (_figuresQueue.Count > clearAfter)
        {
            var oldFigure = _figuresQueue.Dequeue();
            figuresCanvas.Children.Remove(oldFigure);
            _figuresList.Remove(oldFigure);
        }

        // Apply fade-out if enabled
        var fadeAway = _settingsService.Get(SettingsKeys.FadeAway, SettingsDefaults.FadeAway);
        if (fadeAway)
        {
            var fadeAfter = _settingsService.Get(SettingsKeys.FadeAfter, SettingsDefaults.FadeAfter);
            ApplyFadeAnimation(figure, fadeAfter);
        }
    }

    private void ApplySpawnAnimation(Control figure)
    {
        // Match Windows: scale 0→1 AND rotate 360→0 simultaneously with random easing
        var transformGroup = new TransformGroup();
        var scaleTransform = new ScaleTransform(0, 0);
        var rotateTransform = new RotateTransform(360);
        transformGroup.Children.Add(scaleTransform);
        transformGroup.Children.Add(rotateTransform);
        
        figure.RenderTransform = transformGroup;
        figure.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        
        // Use random easing type like Windows
        var easingType = BabySmashUtils.RandomBetweenTwoNumbers(0, 4);
        
        var startTime = DateTime.Now;
        var duration = TimeSpan.FromSeconds(1.0); // 1 second like Windows
        
        var timer = CreateAnimationTimer(TimeSpan.FromMilliseconds(16));
        timer.Tick += (s, e) =>
        {
            var elapsed = DateTime.Now - startTime;
            var progress = Math.Min(1.0, elapsed.TotalMilliseconds / duration.TotalMilliseconds);
            
            // Apply easing
            var easedProgress = ApplyEasing(progress, easingType);
            
            // Scale: 0 → 1
            scaleTransform.ScaleX = easedProgress;
            scaleTransform.ScaleY = easedProgress;
            
            // Rotate: 360 → 0
            rotateTransform.Angle = 360 * (1 - easedProgress);
            
            if (progress >= 1.0)
            {
                RemoveAnimationTimer(timer);
                scaleTransform.ScaleX = 1.0;
                scaleTransform.ScaleY = 1.0;
                rotateTransform.Angle = 0;
            }
        };
        timer.Start();
    }

    private static double ApplyEasing(double t, int easingType)
    {
        return easingType switch
        {
            0 => BounceEaseOut(t),
            1 => ElasticEaseOut(t),
            2 => BackEaseOut(t),
            3 => QuadEaseOut(t),
            _ => t // Linear
        };
    }

    private static double BounceEaseOut(double t)
    {
        // Bounce ease out
        if (t < 1 / 2.75)
        {
            return 7.5625 * t * t;
        }
        else if (t < 2 / 2.75)
        {
            t -= 1.5 / 2.75;
            return 7.5625 * t * t + 0.75;
        }
        else if (t < 2.5 / 2.75)
        {
            t -= 2.25 / 2.75;
            return 7.5625 * t * t + 0.9375;
        }
        else
        {
            t -= 2.625 / 2.75;
            return 7.5625 * t * t + 0.984375;
        }
    }

    private static double ElasticEaseOut(double t)
    {
        if (t == 0 || t == 1) return t;
        double p = 0.3;
        double s = p / 4;
        return Math.Pow(2, -10 * t) * Math.Sin((t - s) * (2 * Math.PI) / p) + 1;
    }

    private static double BackEaseOut(double t)
    {
        double s = 1.70158;
        t -= 1;
        return t * t * ((s + 1) * t + s) + 1;
    }

    private static double QuadEaseOut(double t)
    {
        return -t * (t - 2);
    }

    private void AnimateLettersIntoWord(string word)
    {
        // Find the letter figures that make up this word (the last N figures that are CoolLetters)
        var letterFigures = new List<CoolLetter>();
        var figureList = _figuresList.ToArray();
        
        for (int i = figureList.Length - 1; i >= 0 && letterFigures.Count < word.Length; i--)
        {
            if (figureList[i] is CoolLetter letter)
            {
                letterFigures.Insert(0, letter);
            }
            else
            {
                break; // Stop if we hit a non-letter
            }
        }
        
        if (letterFigures.Count < word.Length)
            return; // Not enough letters
        
        // Calculate word center (average position of letters)
        double centerX = letterFigures.Average(l => Canvas.GetLeft(l) + l.Width / 2);
        double centerY = letterFigures.Average(l => Canvas.GetTop(l) + l.Height / 2);
        
        // Calculate total word width
        double totalWidth = letterFigures.Sum(l => l.Width);
        double leftEdge = centerX - totalWidth / 2;
        
        // Animate each letter to its position in the word
        var duration = TimeSpan.FromMilliseconds(1200);
        double currentX = leftEdge;
        
        foreach (var letter in letterFigures)
        {
            double startX = Canvas.GetLeft(letter);
            double startY = Canvas.GetTop(letter);
            double targetX = currentX;
            double targetY = centerY - letter.Height / 2;
            
            var startTime = DateTime.Now;
            var timer = CreateAnimationTimer(TimeSpan.FromMilliseconds(16));
            
            var capturedLetter = letter;
            var capturedTargetX = targetX;
            var capturedTargetY = targetY;
            var capturedStartX = startX;
            var capturedStartY = startY;
            var capturedTimer = timer;
            
            timer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - startTime;
                var progress = Math.Min(1.0, elapsed.TotalMilliseconds / duration.TotalMilliseconds);
                var easedProgress = QuadEaseOut(progress);
                
                double newX = capturedStartX + (capturedTargetX - capturedStartX) * easedProgress;
                double newY = capturedStartY + (capturedTargetY - capturedStartY) * easedProgress;
                
                Canvas.SetLeft(capturedLetter, newX);
                Canvas.SetTop(capturedLetter, newY);
                
                if (progress >= 1.0)
                {
                    RemoveAnimationTimer(capturedTimer);
                }
            };
            timer.Start();
            
            currentX += letter.Width;
        }
    }

    private void ApplyFadeAnimation(Control figure, double fadeAfterSeconds)
    {
        // Simple fade using timer
        var startTime = DateTime.Now;
        var duration = TimeSpan.FromSeconds(fadeAfterSeconds);
        
        var timer = CreateAnimationTimer(TimeSpan.FromMilliseconds(50));
        timer.Tick += (s, e) =>
        {
            var elapsed = DateTime.Now - startTime;
            var progress = Math.Min(1.0, elapsed.TotalMilliseconds / duration.TotalMilliseconds);
            
            figure.Opacity = 1.0 - progress;
            
            if (progress >= 1.0)
            {
                RemoveAnimationTimer(timer);
                figure.Opacity = 0;
            }
        };
        timer.Start();
    }

    private void PlaySound(FigureTemplate template)
    {
        var soundMode = _settingsService.Get(SettingsKeys.Sounds, SettingsDefaults.Sounds);
        
        if (soundMode == "Laughter")
        {
            _audioService.PlaySound(BabySmashUtils.GetRandomSoundFile());
        }
        else if (soundMode == "Speech")
        {
            if (template.IsLetter)
            {
                _ttsService.Speak(template.Letter);
            }
            else
            {
                _ttsService.Speak($"{template.Color.Name} {template.ShapeType}");
            }
        }
        // "None" - don't play anything
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var mouseDraw = _settingsService.Get(SettingsKeys.MouseDraw, SettingsDefaults.MouseDraw);
        
        // Check if we clicked on an existing figure
        var point = e.GetPosition(figuresCanvas);
        var clickedFigure = FindFigureAtPoint(point);
        
        if (clickedFigure != null && clickedFigure.Opacity > 0.1)
        {
            // Apply random animation effect to the clicked figure
            ApplyRandomClickAnimation(clickedFigure);
            _audioService.PlaySound(BabySmashUtils.GetRandomSoundFile()); // Play laughter
            _isDrawing = true; // Prevent mouse draw on this click
            return;
        }
        
        if (_isDrawing || mouseDraw) return;

        MouseDraw(point);
        _isDrawing = true;
        e.Pointer.Capture(this);
        
        _audioService.PlaySound("smallbumblebee.wav");
    }

    private Control? FindFigureAtPoint(Point point)
    {
        // Search figures in reverse order (topmost first)
        for (int i = _figuresList.Count - 1; i >= 0; i--)
        {
            var figure = _figuresList[i];
            var left = Canvas.GetLeft(figure);
            var top = Canvas.GetTop(figure);
            var bounds = new Rect(left, top, figure.Bounds.Width, figure.Bounds.Height);
            
            if (bounds.Contains(point))
            {
                return figure;
            }
        }
        return null;
    }

    private void ApplyRandomClickAnimation(Control figure)
    {
        var animationType = BabySmashUtils.RandomBetweenTwoNumbers(0, 3);
        
        switch (animationType)
        {
            case 0:
                ApplyJiggleAnimation(figure);
                break;
            case 1:
                ApplyThrobAnimation(figure);
                break;
            case 2:
                ApplyRotateAnimation(figure);
                break;
            case 3:
                ApplySnapAnimation(figure);
                break;
        }
    }

    private void ApplyJiggleAnimation(Control figure)
    {
        figure.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        var rotateTransform = new RotateTransform(0);
        figure.RenderTransform = rotateTransform;
        
        var keyframes = new[] { 0.0, 10.0, 0.0, -10.0, 0.0, 5.0, 0.0, -5.0, 0.0 };
        AnimateWithKeyframes(rotateTransform, keyframes, angle => rotateTransform.Angle = angle, TimeSpan.FromSeconds(0.5));
    }

    private void ApplyThrobAnimation(Control figure)
    {
        figure.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        var scaleTransform = new ScaleTransform(1, 1);
        figure.RenderTransform = scaleTransform;
        
        var keyframes = new[] { 1.0, 1.1, 1.0, 0.9, 1.0, 1.05, 1.0, 0.95, 1.0 };
        AnimateWithKeyframes(scaleTransform, keyframes, scale =>
        {
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
        }, TimeSpan.FromSeconds(0.5));
    }

    private void ApplyRotateAnimation(Control figure)
    {
        figure.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        var rotateTransform = new RotateTransform(0);
        figure.RenderTransform = rotateTransform;
        
        var keyframes = new[] { 0.0, -5.0, 0.0, 90.0, 180.0, 270.0, 360.0, 365.0, 360.0 };
        AnimateWithKeyframes(rotateTransform, keyframes, angle => rotateTransform.Angle = angle, TimeSpan.FromSeconds(1.0));
    }

    private void ApplySnapAnimation(Control figure)
    {
        figure.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        var scaleTransform = new ScaleTransform(1, 1);
        figure.RenderTransform = scaleTransform;
        
        var keyframes = new[] { 1.0, 0.0, 1.0 };
        AnimateWithKeyframes(scaleTransform, keyframes, scale => scaleTransform.ScaleY = scale, TimeSpan.FromSeconds(0.3));
    }

    private void AnimateWithKeyframes<T>(T transform, double[] keyframes, Action<double> setValue, TimeSpan duration) where T : class
    {
        var startTime = DateTime.Now;
        var timer = CreateAnimationTimer(TimeSpan.FromMilliseconds(16));
        
        timer.Tick += (s, e) =>
        {
            var elapsed = DateTime.Now - startTime;
            var progress = Math.Min(1.0, elapsed.TotalMilliseconds / duration.TotalMilliseconds);
            
            // Interpolate between keyframes
            var frameIndex = progress * (keyframes.Length - 1);
            var lowerIndex = (int)Math.Floor(frameIndex);
            var upperIndex = Math.Min(lowerIndex + 1, keyframes.Length - 1);
            var frameFraction = frameIndex - lowerIndex;
            
            var value = keyframes[lowerIndex] + (keyframes[upperIndex] - keyframes[lowerIndex]) * frameFraction;
            setValue(value);
            
            if (progress >= 1.0)
            {
                RemoveAnimationTimer(timer);
                setValue(keyframes[keyframes.Length - 1]);
            }
        };
        timer.Start();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDrawing = false;
        e.Pointer.Capture(null);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isOptionsDialogShown) return;
        
        var point = e.GetPosition(this);
        
        // Update custom cursor position
        if (_customCursor != null)
        {
            Canvas.SetLeft(_customCursor, point.X - 5);
            Canvas.SetTop(_customCursor, point.Y - 5);
        }
        
        var mouseDraw = _settingsService.Get(SettingsKeys.MouseDraw, SettingsDefaults.MouseDraw);
        
        if (_isDrawing || mouseDraw)
        {
            MouseDraw(point);
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            _audioService.PlaySound("rising.wav");
        }
        else
        {
            _audioService.PlaySound("falling.wav");
        }
    }

    private void MouseDraw(Point p)
    {
        var shape = new Ellipse
        {
            Width = 50,
            Height = 50,
            Fill = CreateGradientBrush(BabySmashUtils.GetRandomColor()),
            StrokeThickness = 0
        };

        _mouseEllipsesQueue.Enqueue(shape);
        mouseDragCanvas.Children.Add(shape);
        Canvas.SetLeft(shape, p.X - 25);
        Canvas.SetTop(shape, p.Y - 25);

        var mouseDraw = _settingsService.Get(SettingsKeys.MouseDraw, SettingsDefaults.MouseDraw);
        if (mouseDraw)
        {
            _audioService.PlaySound("smallbumblebee.wav");
        }

        // Keep queue at reasonable size
        while (_mouseEllipsesQueue.Count > 30)
        {
            var shapeToRemove = _mouseEllipsesQueue.Dequeue();
            mouseDragCanvas.Children.Remove(shapeToRemove);
        }
    }

    private static IBrush CreateGradientBrush(BabySmashColor color)
    {
        var lighter = color.LightenOrDarken(50);
        var darker = color.LightenOrDarken(-50);
        
        return new RadialGradientBrush
        {
            GradientOrigin = new RelativePoint(0.75, 0.25, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromRgb(lighter.R, lighter.G, lighter.B), 0),
                new GradientStop(Color.FromRgb(color.R, color.G, color.B), 0.5),
                new GradientStop(Color.FromRgb(darker.R, darker.G, darker.B), 1)
            }
        };
    }

    private async void ShowOptionsDialog()
    {
        _isOptionsDialogShown = true;
        Topmost = false;
        
        try
        {
            var optionsWindow = new OptionsWindow();
            optionsWindow.Topmost = true;
            await optionsWindow.ShowDialog(this);
        }
        finally
        {
            _isOptionsDialogShown = false;
            Topmost = true;
            Activate();
            Focus();
        }
    }
}