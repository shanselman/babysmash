using System;
using System.Collections.Generic;
using System.Globalization;

namespace BabySmash.Globalization
{
    internal static class LvCultureHelper
    {
        private static readonly HashSet<string> FeminineShapes = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
        {
            "Trapece",
            "Zvaigzne",
            "Sirds",
        };

        private static readonly HashSet<string> AlwaysFeminineKeys = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
        {
            "Rozā",
        };


        public static bool IsLatvian(this CultureInfo cultureInfo)
        {
            return cultureInfo?.TwoLetterISOLanguageName == "lv";
        }

        internal static string GenderizeTemplate(string key, string template)
        {
            if (String.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (AlwaysFeminineKeys.Contains(key))
            {
                return key;
            }

            if (String.IsNullOrEmpty(template))
            {
                return key;
            }

            if (FeminineShapes.Contains(template))
            {
                return key.Substring(0, key.Length - 1) + "a";
            }

            return key;
        }
    }
}
