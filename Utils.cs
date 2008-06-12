using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace BabySmash
{
    class Utils
    {
        private static Brush[] someBrushes = { Brushes.Red,
                                  Brushes.Blue,
                                  Brushes.Yellow,
                                  Brushes.Green,
                                  Brushes.Purple,
                                  Brushes.Pink,
                                  Brushes.Orange,
                                  Brushes.Tan,
                                  Brushes.Gray};

        private static Dictionary<Brush, string> brushToString = new Dictionary<Brush, string> { 
                                  { Brushes.Red, "Red" },
                                  { Brushes.Blue, "Blue" },
                                  { Brushes.Yellow, "Yellow" },
                                  { Brushes.Green, "Green" },
                                  { Brushes.Purple, "Purple" },
                                  { Brushes.Pink, "Pink" },
                                  { Brushes.Orange, "Orange" },
                                  { Brushes.Tan, "Tan" },
                                  { Brushes.Gray, "Gray" }
                                 };

        private static string[] sounds = { "giggle.wav", 
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
            else
                return true;
        }

        private static Random lRandom = new Random();

        public static int RandomBetweenTwoNumbers(int min, int max)
        {
            return lRandom.Next(min, max + 1);
        }
    }
}
