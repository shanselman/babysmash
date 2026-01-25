using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using BabySmash.Core.Models;

namespace BabySmash.Linux.Shapes;

/// <summary>
/// Base class for all BabySmash shapes with face support
/// </summary>
public abstract class ShapeBase : UserControl, IHasFace
{
    protected Canvas? FaceCanvas;
    protected Canvas? EyesCanvas;
    private DispatcherTimer? _blinkTimer;

    public bool FaceVisible
    {
        get => FaceCanvas?.IsVisible ?? false;
        set
        {
            if (FaceCanvas != null)
                FaceCanvas.IsVisible = value;
        }
    }

    protected void InitializeFace(string faceCanvasName = "Face", string? eyesCanvasName = null)
    {
        FaceCanvas = this.FindControl<Canvas>(faceCanvasName);
        if (eyesCanvasName != null)
            EyesCanvas = this.FindControl<Canvas>(eyesCanvasName);
        
        StartBlinkAnimation();
    }

    private void StartBlinkAnimation()
    {
        var eyesTarget = EyesCanvas ?? FaceCanvas;
        if (eyesTarget == null) return;

        // Find eye elements - try common names
        var eyes = new List<Control>();
        for (int i = 1; i <= 2; i++)
        {
            var eye = this.FindControl<Control>($"Eye{i}");
            if (eye != null) eyes.Add(eye);
        }
        
        // If no individual eyes, blink the whole eyes canvas
        if (eyes.Count == 0 && EyesCanvas != null)
        {
            eyes.Add(EyesCanvas);
        }

        if (eyes.Count == 0) return;

        _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.1 + Random.Shared.NextDouble() * 5) };
        _blinkTimer.Tick += async (s, e) =>
        {
            foreach (var eye in eyes)
                eye.IsVisible = false;
            
            await Task.Delay(200);
            
            foreach (var eye in eyes)
                eye.IsVisible = true;
            
            // Randomize next blink
            _blinkTimer.Interval = TimeSpan.FromSeconds(2.1 + Random.Shared.NextDouble() * 5);
        };
        _blinkTimer.Start();
    }

    protected static IBrush CreateGradientBrush(BabySmashColor color)
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
}
