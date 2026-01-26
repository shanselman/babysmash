using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using BabySmash.Linux.Core.Models;

namespace BabySmash.Linux.Shapes;

public partial class CoolTrapezoid : ShapeBase
{
    private Path? _body;

    public CoolTrapezoid()
    {
        InitializeComponent();
        _body = this.FindControl<Path>("Body");
        InitializeFace("Face");
    }

    public CoolTrapezoid(BabySmashColor color) : this()
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
