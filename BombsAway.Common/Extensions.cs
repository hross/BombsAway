using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common
{
    public static class Extensions
    {
        public static bool IsSimilarTo(this Color thisColor, Color color, int maxDeltaE = 20)
        {
            var a = new Rgb { R = thisColor.R, G = thisColor.G, B = thisColor.B };
            var b = new Rgb { R = color.R, G = color.G, B = color.B };
            var deltaE = a.Compare(b, new Cie1976Comparison());

            return deltaE < maxDeltaE;

            //return 
            //    Math.Abs(color.R - thisColor.R) < 30 && 
            //    Math.Abs(color.G - thisColor.G) < 30 &&
            //    Math.Abs(color.B - thisColor.B) < 30;

            //return EuclideanDistance(thisColor, color) < 40;
        }

        public static double EuclideanDistance(Color color1, Color color2)
        {
            var red = Math.Pow(Convert.ToDouble(color1.R - color2.R), 2.0);
            var green = Math.Pow(Convert.ToDouble(color1.G - color2.G), 2.0);
            var blue = Math.Pow(Convert.ToDouble(color1.B - color2.B), 2.0);

            var result = Math.Sqrt(blue + green + red);

            return Math.Abs(result);
        }

        public static string HexString(this Color color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }


        #region public color comparison extensions

        public static ColorName NameInBomberman(this Color thisColor)
        {
            if (thisColor.G.IsClearlyLessThan(thisColor.R) && thisColor.G.IsClearlyLessThan(thisColor.B)
                && thisColor.IsVeryBright())
            {
                return ColorName.Pink;
            }
            else if (thisColor.G.IsVirtuallySameAs(thisColor.R) && thisColor.G.IsVirtuallySameAs(thisColor.B)
                && thisColor.IsVeryBright())
            {
                return ColorName.White;
            }
            else if (thisColor.R.IsClearlyLessThan(thisColor.G) && thisColor.B.IsClearlyLessThan(thisColor.G)
                && thisColor.IsVeryDark())
            {
                return ColorName.DarkGreen;
            }
            else if (thisColor.G.IsVirtuallySameAs(thisColor.R) && thisColor.G.IsVirtuallySameAs(thisColor.B)
                && !thisColor.IsVeryBright() && !thisColor.IsVeryDark())
            {
                return ColorName.Grey;
            }
            else if (thisColor.G.IsClearlyLessThan(thisColor.R) && thisColor.B.IsClearlyLessThan(thisColor.R)
                && thisColor.IsVeryDark())
            {
                return ColorName.Brown;
            }
            else if (thisColor.R.IsClearlyLessThan(thisColor.G) && thisColor.B.IsClearlyLessThan(thisColor.G)
                && !thisColor.IsVeryBright() && !thisColor.IsVeryDark())
            {
                return ColorName.LightGreen;
            }
            else if (thisColor.G.IsClearlyLessThan(thisColor.B) && thisColor.R.IsClearlyLessThan(thisColor.B)
                && thisColor.IsVeryDark())
            {
                return ColorName.DarkBlue;
            }
            else if (thisColor.B.IsClearlyLessThan(thisColor.R) && thisColor.B.IsClearlyLessThan(thisColor.G)
                && !thisColor.IsVeryBright() && !thisColor.IsVeryDark())
            {
                return ColorName.Orange;
            }
            else if (thisColor.R.IsClearlyLessThan(thisColor.B) && thisColor.R.IsClearlyLessThan(thisColor.G)
                && thisColor.IsVeryBright())
            {
                return ColorName.BabyBlue;
            }
            if (thisColor.G.IsClearlyLessThan(thisColor.R) && thisColor.G.IsClearlyLessThan(thisColor.B)
                && !thisColor.IsVeryBright() && !thisColor.IsVeryDark())
            {
                return ColorName.Purple;
            }
            else if (thisColor.B.IsClearlyLessThan(thisColor.R) && thisColor.B.IsClearlyLessThan(thisColor.G)
                && thisColor.IsVeryBright())
            {
                return ColorName.Yellow;
            }
            else if (thisColor.G.IsVirtuallySameAs(thisColor.R) && thisColor.G.IsVirtuallySameAs(thisColor.B)
                && thisColor.IsVeryDark())
            {
                return ColorName.Black;
            }
            if (thisColor.G.IsClearlyLessThan(thisColor.R) && thisColor.G.IsClearlyLessThan(thisColor.B)
                && thisColor.IsVeryDark())
            {
                return ColorName.DarkPurple;
            }
            else if (thisColor.G.IsClearlyLessThan(thisColor.R) && thisColor.B.IsClearlyLessThan(thisColor.R)
                && !thisColor.IsVeryDark() && !thisColor.IsVeryBright())
            {
                return ColorName.Red;
            }
            else if (thisColor.G.IsClearlyLessThan(thisColor.B) && thisColor.R.IsClearlyLessThan(thisColor.B)
                && !thisColor.IsVeryBright() && !thisColor.IsVeryDark())
            {
                return ColorName.Blue;
            }
            else
            {
                return ColorName.Unknown;
            }

        }
     
        #endregion

        #region private color detection members

        private const int VIRTUALLY_SAME_THRESHOLD = 10;
        private const int VERY_BRIGHT_TOTAL_THRESHOLD = 144 * 3; //432
        private const int VERY_DARK_TOTAL_THRESHOLD = 96 * 3; //288
        private const int VERY_BRIGHT_SINGLE_COLOR_THRESHOLD = 222;

        private static bool IsVirtuallySameAs(this byte colorComponent1, int colorComponent2)
        {
            return AreVirtuallySame(colorComponent1, colorComponent2);
        }

        private static bool AreVirtuallySame(int colorComponent1, int colorComponent2)
        {
            return Math.Abs(colorComponent1 - colorComponent2) <= VIRTUALLY_SAME_THRESHOLD;
        }

        private static bool IsClearlyLessThan(this byte colorComponent1, int colorComponent2)
        {
            int difference = colorComponent2 - colorComponent1;
            return difference > VIRTUALLY_SAME_THRESHOLD;
        }

        private static bool IsVeryBright(this Color color)
        {
            return color.R + color.G + color.B >= VERY_BRIGHT_TOTAL_THRESHOLD && (
                color.R > VERY_BRIGHT_SINGLE_COLOR_THRESHOLD
                || color.G > VERY_BRIGHT_SINGLE_COLOR_THRESHOLD
                || color.B > VERY_BRIGHT_SINGLE_COLOR_THRESHOLD
                );
        }

        private static bool IsVeryDark(this Color color)
        {
            return color.R + color.G + color.B <= VERY_DARK_TOTAL_THRESHOLD;
        }

        #endregion
    }
}
