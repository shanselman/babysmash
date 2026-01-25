using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using BabySmash.Core.Models;

namespace BabySmash.Linux.Shapes;

public partial class CoolSquare : ShapeBase
{
    private Rectangle? _body;

    public CoolSquare()
    {
        InitializeComponent();
        _body = this.FindControl<Rectangle>("Body");
        InitializeFace("Face");
    }

    public CoolSquare(BabySmashColor color) : this()
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
