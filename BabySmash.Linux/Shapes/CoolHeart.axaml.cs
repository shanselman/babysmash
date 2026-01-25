using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using BabySmash.Core.Models;

namespace BabySmash.Linux.Shapes;

public partial class CoolHeart : ShapeBase
{
    private Path? _body;

    public CoolHeart()
    {
        InitializeComponent();
        _body = this.FindControl<Path>("Body");
        InitializeFace("Face");
    }

    public CoolHeart(BabySmashColor color) : this()
    {
        SetColor(color);
    }

    public void SetColor(BabySmashColor color)
    {
        if (_body != null)
        {
            _body.Fill = CreateGradientBrush(color);
        }
    }
}
