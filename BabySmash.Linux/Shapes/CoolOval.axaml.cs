using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using BabySmash.Linux.Core.Models;

namespace BabySmash.Linux.Shapes;

public partial class CoolOval : ShapeBase
{
    private Ellipse? _body;

    public CoolOval()
    {
        InitializeComponent();
        _body = this.FindControl<Ellipse>("Body");
        InitializeFace("Face");
    }

    public CoolOval(BabySmashColor color) : this()
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
