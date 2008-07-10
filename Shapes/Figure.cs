using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using BabySmash.Properties;

namespace BabySmash
{
    public interface IHasFace
    {
        Visibility FaceVisible { get; set; }
    }

}