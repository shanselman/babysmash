using System;
using System.Diagnostics;
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

        public CoolLetter(Brush x, char letter) : this()
        {
            this.Character = letter;
            this.letterPath.Fill = x;

            this.letterPath.Data = MakeCharacterGeometry(GetLetterCharacter(letter));
            this.Width = this.letterPath.Data.Bounds.Width + this.letterPath.Data.Bounds.X + this.letterPath.StrokeThickness / 2;
            this.Height = this.letterPath.Data.Bounds.Height + this.letterPath.Data.Bounds.Y + this.letterPath.StrokeThickness / 2;
        }

        public char Character { get; private set; }

        private static Geometry MakeCharacterGeometry(char character)
        {
            var fontFamily = new FontFamily(Settings.Default.FontFamily);
            var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Heavy, FontStretches.Normal);
            var formattedText = new FormattedText(
                character.ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                300,
                Brushes.Black);
            return formattedText.BuildGeometry(new Point(0, 0)).GetAsFrozen() as Geometry;
        }

        private static char GetLetterCharacter(char name)
        {
            Debug.Assert(name == char.ToUpperInvariant(name), "Always provide uppercase character names to this method.");

            if (Settings.Default.ForceUppercase)
            {
                return name;
            }

            // Return a random uppercase or lowercase letter.
            return Utils.GetRandomBoolean() ? name : char.ToLowerInvariant(name);
        }
    }
}