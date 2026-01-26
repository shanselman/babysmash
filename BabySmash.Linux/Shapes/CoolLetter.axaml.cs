using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using BabySmash.Linux.Core.Models;
using BabySmash.Linux.Core.Services;

namespace BabySmash.Linux.Shapes;

public partial class CoolLetter : UserControl, ILetterFigure
{
    private TextBlock? _letterText;
    
    public char Character { get; private set; }

    public CoolLetter()
    {
        InitializeComponent();
        _letterText = this.FindControl<TextBlock>("LetterText");
    }

    public CoolLetter(BabySmashColor color, char letter) : this()
    {
        Character = letter;
        SetLetter(letter, color);
    }

    public void SetLetter(char letter, BabySmashColor color)
    {
        Character = char.ToUpperInvariant(letter); // Always store uppercase for word matching
        
        if (_letterText != null)
        {
            // Randomly choose uppercase or lowercase for display
            char displayChar = BabySmashUtils.GetRandomBoolean() 
                ? char.ToUpperInvariant(letter) 
                : char.ToLowerInvariant(letter);
            
            _letterText.Text = displayChar.ToString();
            _letterText.Foreground = CreateGradientBrush(color);
        }
        
        // Size needs to accommodate FontSize 300 (matching Windows behavior)
        Width = 350;
        Height = 400;
    }

    private static IBrush CreateGradientBrush(BabySmashColor color)
    {
        var lighter = color.LightenOrDarken(50);
        var darker = color.LightenOrDarken(-50);
        
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromRgb(lighter.R, lighter.G, lighter.B), 0),
                new GradientStop(Color.FromRgb(color.R, color.G, color.B), 0.5),
                new GradientStop(Color.FromRgb(darker.R, darker.G, darker.B), 1)
            }
        };
    }
}
