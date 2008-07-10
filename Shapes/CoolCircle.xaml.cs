using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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