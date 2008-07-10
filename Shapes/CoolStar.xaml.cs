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