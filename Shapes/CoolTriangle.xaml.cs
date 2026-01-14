using System;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;

namespace BabySmash
{
    /// <summary>
    /// Interaction logic for CoolTriangle.xaml
    /// </summary>
    [Serializable]
    public partial class CoolTriangle : IHasFace
    {
        public CoolTriangle(Brush x) : this()
        {
            Body.Fill = x;
        }

        public CoolTriangle()
        {
            this.InitializeComponent();
        }
        
        public Visibility FaceVisible
        {
            get
            {
                return Face.Visibility;
            }
            set
            {
                Face.Visibility = value;
            }
        }
    }
}