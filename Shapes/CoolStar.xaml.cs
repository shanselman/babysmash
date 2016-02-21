using System;
using System.Windows;
using System.Windows.Media;

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

        #region IHasFace Members

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

        #endregion
    }
}