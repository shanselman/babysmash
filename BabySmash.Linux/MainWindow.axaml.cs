using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using BabySmash.Core.Interfaces;
using BabySmash.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BabySmash.Linux;

public partial class MainWindow : Window
{
    private readonly ITtsService _ttsService;
    private readonly IAudioService _audioService;
    private readonly WordFinder _wordFinder;
    private readonly Random _random = new();
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Get services from DI
        _ttsService = App.Services.GetRequiredService<ITtsService>();
        _audioService = App.Services.GetRequiredService<IAudioService>();
        _wordFinder = App.Services.GetRequiredService<WordFinder>();
        
        // Hook up keyboard events
        KeyDown += OnKeyDown;
        
        // Focus the window
        Loaded += (s, e) => Focus();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Exit on Escape
        if (e.Key == Key.Escape)
        {
            Close();
            return;
        }

        // Get the character if it's a letter or number
        var key = e.Key;
        char? character = null;
        
        if (key >= Key.A && key <= Key.Z)
        {
            character = (char)('A' + (key - Key.A));
        }
        else if (key >= Key.D0 && key <= Key.D9)
        {
            character = (char)('0' + (key - Key.D0));
        }

        if (character.HasValue)
        {
            // Create a shape
            CreateShape(character.Value);
            
            // Speak the letter
            _ttsService.Speak(character.Value.ToString());
            
            // Check for words
            var word = _wordFinder.AddLetter(character.Value);
            if (word != null)
            {
                _ttsService.Speak($"You spelled {word}!");
            }
        }

        e.Handled = true;
    }

    private void CreateShape(char character)
    {
        // Create a simple ellipse shape
        var shape = new Ellipse
        {
            Width = 100,
            Height = 100,
            Fill = GetRandomBrush(),
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };

        // Position randomly on screen
        var x = _random.Next(0, (int)Math.Max(1, Bounds.Width - 100));
        var y = _random.Next(0, (int)Math.Max(1, Bounds.Height - 100));
        
        Canvas.SetLeft(shape, x);
        Canvas.SetTop(shape, y);

        // Add to canvas
        figuresCanvas.Children.Add(shape);

        // Add text label
        var text = new TextBlock
        {
            Text = character.ToString(),
            FontSize = 48,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        };
        
        Canvas.SetLeft(text, x + 30);
        Canvas.SetTop(text, y + 20);
        figuresCanvas.Children.Add(text);
    }

    private IBrush GetRandomBrush()
    {
        var colors = new[]
        {
            Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow,
            Colors.Orange, Colors.Purple, Colors.Pink, Colors.Cyan
        };
        return new SolidColorBrush(colors[_random.Next(colors.Length)]);
    }
}