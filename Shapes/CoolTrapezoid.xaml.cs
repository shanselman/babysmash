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
   /// Interaction logic for CoolTrapezoid.xaml
   /// </summary>
   [Serializable]
   public partial class CoolTrapezoid : IHasFace
   {
      public CoolTrapezoid(Brush x)
         : this()
      {
         Body.Fill = x;
      }

      public CoolTrapezoid()
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