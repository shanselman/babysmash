using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using BabySmash.Core.Models;

namespace BabySmash.Linux.Shapes;

public partial class CoolTriangle : ShapeBase
{
    private Path? _body;

    public CoolTriangle()
    {
        InitializeComponent();
        _body = this.FindControl<Path>("Body");
        InitializeFace("Face");
    }

    public CoolTriangle(BabySmashColor color) : this()
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
