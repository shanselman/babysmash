using System;
using System.Windows;
using System.Windows.Media;

namespace BabySmash
{
    /// <summary>
    /// Interaction logic for CoolHexagon.xaml
    /// </summary>
    [Serializable]
    public partial class CoolHexagon : IHasFace
    {
        public CoolHexagon(Brush x) : this()
        {
            this.Body.Fill = x;
        }

        public CoolHexagon()
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