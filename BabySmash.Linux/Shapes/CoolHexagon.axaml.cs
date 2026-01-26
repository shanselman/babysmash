using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using BabySmash.Linux.Core.Models;

namespace BabySmash.Linux.Shapes;

public partial class CoolHexagon : ShapeBase
{
    private Path? _body;

    public CoolHexagon()
    {
        InitializeComponent();
        _body = this.FindControl<Path>("Body");
        InitializeFace("Face");
    }

    public CoolHexagon(BabySmashColor color) : this()
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
