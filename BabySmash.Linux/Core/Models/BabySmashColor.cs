using System;
namespace BabySmash.Linux.Core.Models;

/// <summary>
/// Platform-independent color representation
/// </summary>
public readonly struct BabySmashColor
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }
    public string Name { get; }

    public BabySmashColor(byte r, byte g, byte b, string name, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
        Name = name;
    }

    public BabySmashColor LightenOrDarken(sbyte degree)
    {
        return new BabySmashColor(
            (byte)Math.Max(Math.Min(R + degree, 255), 0),
            (byte)Math.Max(Math.Min(G + degree, 255), 0),
            (byte)Math.Max(Math.Min(B + degree, 255), 0),
            Name,
            A
        );
    }

    // Predefined colors that match the Windows version
    public static readonly BabySmashColor Red = new(255, 0, 0, "Red");
    public static readonly BabySmashColor Blue = new(0, 0, 255, "Blue");
    public static readonly BabySmashColor Yellow = new(255, 255, 0, "Yellow");
    public static readonly BabySmashColor Green = new(0, 128, 0, "Green");
    public static readonly BabySmashColor Purple = new(128, 0, 128, "Purple");
    public static readonly BabySmashColor Pink = new(255, 192, 203, "Pink");
    public static readonly BabySmashColor Orange = new(255, 165, 0, "Orange");
    public static readonly BabySmashColor Tan = new(210, 180, 140, "Tan");
    public static readonly BabySmashColor Gray = new(128, 128, 128, "Gray");

    public static readonly BabySmashColor[] AllColors = { Red, Blue, Yellow, Green, Purple, Pink, Orange, Tan, Gray };
}
