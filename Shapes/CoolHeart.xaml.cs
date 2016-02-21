using System;
using System.Windows;
using System.Windows.Media;

namespace BabySmash
{
    /// <summary>
    /// Interaction logic for CoolHeart.xaml
    /// </summary>
    [Serializable]
    public partial class CoolHeart : IHasFace
    {
        public CoolHeart(Brush x) : this()
        {
            this.Body.Fill = x;
        }

        public CoolHeart()
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