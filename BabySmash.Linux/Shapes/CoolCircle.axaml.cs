using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using BabySmash.Core.Models;

namespace BabySmash.Linux.Shapes;

public partial class CoolCircle : ShapeBase
{
    private Ellipse? _body;

    public CoolCircle()
    {
        InitializeComponent();
        _body = this.FindControl<Ellipse>("Body");
        InitializeFace("Face");
    }

    public CoolCircle(BabySmashColor color) : this()
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
