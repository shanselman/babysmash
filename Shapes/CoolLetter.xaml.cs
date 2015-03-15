using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using BabySmash.Properties;

namespace BabySmash
{
    /// <summary>   
    /// Interaction logic for CoolLetter.xaml
    /// </summary>
    [Serializable]
    public partial class CoolLetter
    {
        public CoolLetter()
        {
            this.InitializeComponent();
        }

        public CoolLetter(Brush x, string letter)
            : this()
        {
            this.letterPath.Fill = x;

            this.letterPath.Data = MakeCharacterGeometry(GetLetterCharacter(letter));
            this.Width = this.letterPath.Data.Bounds.Width + this.letterPath.Data.Bounds.X + this.letterPath.StrokeThickness / 2;
            this.Height = this.letterPath.Data.Bounds.Height + this.letterPath.Data.Bounds.Y + this.letterPath.StrokeThickness / 2;
        }

        private static Geometry MakeCharacterGeometry(string t)
        {
            var fText = new FormattedText(
                t,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily(Properties.Settings.Default.FontFamily),
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