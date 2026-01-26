using System;
using BabySmash.Linux.Core.Models;

namespace BabySmash.Linux.Core.Services;

/// <summary>
/// Shared utility functions for BabySmash
/// </summary>
public static class BabySmashUtils
{
    private static readonly Random _random = new();
    private static readonly object _lock = new();

    private static readonly string[] LaughterSounds =
    {
        "giggle.wav",
        "babylaugh.wav",
        "babygigl2.wav",
        "ccgiggle.wav",
        "laughingmice.wav",
        "scooby2.wav",
    };

    public static BabySmashColor GetRandomColor()
    {
        lock (_lock)
        {
            return BabySmashColor.AllColors[_random.Next(0, BabySmashColor.AllColors.Length)];
        }
    }

    public static string GetRandomSoundFile()
    {
        lock (_lock)
        {
            return LaughterSounds[_random.Next(0, LaughterSounds.Length)];
        }
    }

    public static bool GetRandomBoolean()
    {
        lock (_lock)
        {
            return _random.Next(0, 2) != 0;
        }
    }

    public static int RandomBetweenTwoNumbers(int min, int max)
    {
        lock (_lock)
        {
            return _random.Next(min, max + 1);
        }
    }

    public static ShapeType GetRandomShapeType()
    {
        var shapes = Enum.GetValues<ShapeType>();
        lock (_lock)
        {
            return shapes[_random.Next(0, shapes.Length)];
        }
    }

    /// <summary>
    /// Generates a figure template for the given character
    /// </summary>
    public static FigureTemplate GenerateFigureTemplate(char displayChar)
    {
        var color = GetRandomColor();
        var shapeType = GetRandomShapeType();
        
        return new FigureTemplate
        {
            Color = color,
            ShapeType = shapeType,
            Letter = displayChar.ToString(),
            Name = char.IsLetterOrDigit(displayChar) 
                ? displayChar.ToString() 
                : shapeType.ToString()
        };
    }
}
