using System;
using System.Windows;
using System.Windows.Media;

namespace BabySmash
{
    /// <summary>   
    /// Interaction logic for CoolCircle.xaml
    /// </summary>
    [Serializable]
    public partial class CoolCircle : IHasFace
    {
        public CoolCircle(Brush x)
            : this()
        {
            this.Body.Fill = x;
        }

        public CoolCircle()
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