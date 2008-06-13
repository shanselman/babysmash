using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace BabySmash
{
    class Utils
    {
        static Utils()
        {
           brushToString = new Dictionary<Brush, string> { { Brushes.Red, "Red" },
                                  { Brushes.Blue, "Blue" },
                                  { Brushes.Yellow, "Yellow" },
                                  { Brushes.Green, "Green" },
                                  { Brushes.Purple, "Purple" },
                                  { Brushes.Pink, "Pink" },
                                  { Brushes.Orange, "Orange" },
                                  { Brushes.Tan, "Tan" },
                                  { Brushes.Gray, "Gray" }
                                 };

            someBrushes = new Brush[brushToString.Count];
            brushToString.Keys.CopyTo(someBrushes, 0);
        }
        private static Brush[] someBrushes;

        private static Dictionary<Brush, string> brushToString;

        private static readonly string[] sounds = { "giggle.wav", 
                                            "babylaugh.wav",
                                            "babygigl2.wav",
                                            "ccgiggle.wav",
                                            "laughingmice.wav",
                                            "scooby2.wav",
                                         };

        public static Brush GetRandomColoredBrush()
        {
            Brush brush = someBrushes[lRandom.Next(0, someBrushes.Length)];
            return  brush;
        }

        public static string BrushToString(Brush b)
        {
           return brushToString[b];
        }

        public static string GetRandomSoundFile()
        {
            string retVal = sounds[lRandom.Next(0, sounds.Length)];
            return ".Resources.Sounds." + retVal;
        }
        public static bool GetRandomBoolean()
        {
            if (lRandom.Next(0, 2) == 0)
                return false;
            return true;
        }

        private static readonly Random lRandom = new Random();

        public static int RandomBetweenTwoNumbers(int min, int max)
        {
            return lRandom.Next(min, max + 1);
        }
    }
}
