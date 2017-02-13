using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace BabySmash
{
    internal static class Utils
    {
        private static readonly Dictionary<Color, string> brushToString;

        private static readonly Random lRandom = new Random(); // BUG BUG: Believe it or not, Random is NOT THREAD SAFE!

        private static readonly FunCursor1 fun1 = new FunCursor1();
        private static readonly FunCursor2 fun2 = new FunCursor2();

        private static readonly Color[] someColors;

        private static readonly string[] sounds = {
                                                      "giggle.wav",
                                                      "babylaugh.wav",
                                                      "babygigl2.wav",
                                                      "ccgiggle.wav",
                                                      "laughingmice.wav",
                                                      "scooby2.wav",
                                                  };

        static Utils()
        {
            brushToString = new Dictionary<Color, string>
                                {
                                    {Colors.Red, "Red"},
                                    {Colors.Blue, "Blue"},
                                    {Colors.Yellow, "Yellow"},
                                    {Colors.Green, "Green"},
                                    {Colors.Purple, "Purple"},
                                    {Colors.Pink, "Pink"},
                                    {Colors.Orange, "Orange"},
                                    {Colors.Tan, "Tan"},
                                    {Colors.Gray, "Gray"}
                                };

            someColors = new Color[brushToString.Count];
            brushToString.Keys.CopyTo(someColors, 0);
        }

        public static Color GetRandomColor()
        {
            Color color = someColors[lRandom.Next(0, someColors.Length)];
            return color;
        }

        public static Brush GetGradientBrush(Color color)
        {
            RadialGradientBrush myBrush = new RadialGradientBrush();
            myBrush.GradientOrigin = new Point(0.75, 0.25);
            myBrush.GradientStops.Add(new GradientStop(color.LightenOrDarken(50), 0.0));
            myBrush.GradientStops.Add(new GradientStop(color, 0.5));
            myBrush.GradientStops.Add(new GradientStop(color.LightenOrDarken(-50), 1.0));
            return myBrush;
        }

        public static Color LightenOrDarken(this Color src, sbyte degree)
        {
            Color ret = new Color();
            ret.A = src.A;
            ret.R = (byte)Math.Max(Math.Min(src.R + degree, 255), 0);
            ret.G = (byte)Math.Max(Math.Min(src.G + degree, 255), 0);
            ret.B = (byte)Math.Max(Math.Min(src.B + degree, 255), 0);
            return ret;
        }


        public static string ColorToString(Color b)
        {
            return brushToString[b];
        }

        public static string GetRandomSoundFile()
        {
            return sounds[lRandom.Next(0, sounds.Length)];
        }

        public static bool GetRandomBoolean()
        {
            if (lRandom.Next(0, 2) == 0)
                return false;
            return true;
        }

        public static int RandomBetweenTwoNumbers(int min, int max)
        {
            return lRandom.Next(min, max + 1);
        }

        internal static System.Windows.Controls.UserControl GetCursor()
        {
            switch (Properties.Settings.Default.CursorType)
            {
                case "Hand":
                    return fun2;
                case "Arrow":
                    return fun1;
            }
            return fun1;
        }
    }
}