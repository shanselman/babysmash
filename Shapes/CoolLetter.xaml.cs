using System;
using System.Collections.Generic;
using System.Globalization;
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
using BabySmash.Properties;

namespace BabySmash
{
	/// <summary>   
	/// Interaction logic for CoolCircle.xaml
	/// </summary>
   [Serializable]
   public partial class CoolLetter 
	{
      public CoolLetter()
      {
         this.InitializeComponent();
      }
		public CoolLetter(Brush x, string letter) : this()
		{
			

		    this.letterPath.Fill = x;

		    this.letterPath.Data = MakeCharacterGeometry(GetLetterCharacter(letter));
		    this.Height = 400;
		}

        private static Geometry MakeCharacterGeometry(string t)
        {
            var fText = new FormattedText(
                t,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily("Arial"),
                    FontStyles.Normal,
                    FontWeights.Heavy,
                    FontStretches.Normal),
                300,
                Brushes.Black
                );

            return fText.BuildGeometry(new Point(0, 0)).GetAsFrozen() as Geometry;
        }

        private static string GetLetterCharacter(string name)
        {
            string nameToDisplay;
            if (Settings.Default.ForceUppercase)
            {
                nameToDisplay = name;
            }
            else
            {
                nameToDisplay = Utils.GetRandomBoolean() ? name : name.ToLowerInvariant();
            }
            return nameToDisplay;
        }

	}
}