namespace BabySmash.Linux.Shapes;

/// <summary>
/// Interface for shapes that have a face that can be shown/hidden
/// </summary>
public interface IHasFace
{
    bool FaceVisible { get; set; }
}
