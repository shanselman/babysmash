namespace BabySmash.Core.Models;

/// <summary>
/// Shape types available in BabySmash
/// </summary>
public enum ShapeType
{
    Circle,
    Oval,
    Rectangle,
    Square,
    Triangle,
    Hexagon,
    Trapezoid,
    Star,
    Heart
}

/// <summary>
/// Platform-independent figure template that describes what to display
/// </summary>
public class FigureTemplate
{
    /// <summary>
    /// The color of the shape
    /// </summary>
    public BabySmashColor Color { get; set; }

    /// <summary>
    /// The name to speak (color + shape or letter)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// If a letter/number was pressed, this contains it
    /// </summary>
    public string Letter { get; set; } = string.Empty;

    /// <summary>
    /// The shape type (for non-letter figures)
    /// </summary>
    public ShapeType ShapeType { get; set; }

    /// <summary>
    /// Whether this is a letter/number or a shape
    /// </summary>
    public bool IsLetter => Letter.Length == 1 && char.IsLetterOrDigit(Letter[0]);
}
