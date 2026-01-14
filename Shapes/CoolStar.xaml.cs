using System;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;

namespace BabySmash
{
    /// <summary>
    /// Interaction logic for CoolStar.xaml
    /// </summary>
    [Serializable]
    public partial class CoolStar : IHasFace
    {
        public CoolStar(Brush x) : this()
        {
            Body.Fill = x;
        }
        
        public CoolStar()
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