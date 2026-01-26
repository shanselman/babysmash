using Avalonia.Controls;
using BabySmash.Linux.Core.Models;
using BabySmash.Linux.Core.Services;

namespace BabySmash.Linux.Shapes;

/// <summary>
/// Factory for creating shape controls from figure templates
/// </summary>
public static class FigureGenerator
{
    public static UserControl CreateFigure(FigureTemplate template)
    {
        if (template.IsLetter)
        {
            return new CoolLetter(template.Color, template.Letter[0]);
        }
        
        return template.ShapeType switch
        {
            ShapeType.Circle => CreateShape<CoolCircle>(template.Color),
            ShapeType.Oval => CreateShape<CoolOval>(template.Color),
            ShapeType.Rectangle => CreateShape<CoolRectangle>(template.Color),
            ShapeType.Square => CreateShape<CoolSquare>(template.Color),
            ShapeType.Triangle => CreateShape<CoolTriangle>(template.Color),
            ShapeType.Hexagon => CreateShape<CoolHexagon>(template.Color),
            ShapeType.Trapezoid => CreateShape<CoolTrapezoid>(template.Color),
            ShapeType.Star => CreateShape<CoolStar>(template.Color),
            ShapeType.Heart => CreateShape<CoolHeart>(template.Color),
            _ => CreateShape<CoolCircle>(template.Color)
        };
    }

    private static T CreateShape<T>(BabySmashColor color) where T : ShapeBase, new()
    {
        var shape = new T();
        
        // Use reflection to call SetColor if it exists
        var setColorMethod = typeof(T).GetMethod("SetColor");
        setColorMethod?.Invoke(shape, new object[] { color });
        
        return shape;
    }

    public static void SetFaceVisibility(UserControl control, bool visible)
    {
        if (control is IHasFace hasFace)
        {
            hasFace.FaceVisible = visible;
        }
    }
}
