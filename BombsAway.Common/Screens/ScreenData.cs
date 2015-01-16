using System.Drawing;

namespace BombsAway.Common.Screens
{
    public sealed class ScreenData
    {
        public static Point Player1Head = new Point(143, 119);
        public static Point Player2Head = new Point(143, 182);
        public static Point Player3Head = new Point(143, 248);
        public static Point Player4Head = new Point(143, 311);

        public static Color TrophyBottomGray = Color.FromArgb(51, 50, 59);
        public static Color BackgroundGreen = Color.FromArgb(53, 108, 56);

        public static Color PlayscreenTopColor = Color.FromArgb(255, 178, 31);

        public static Color PlayerTextColor = Color.FromArgb(210, 146, 0);
        public static Color PlayerTextComColor = Color.FromArgb(182, 53, 0);
        public static Color PlayerTextOffColor = Color.FromArgb(106, 205, 145);
        public static Color PlayerManColor = Color.FromArgb(50, 91, 255);

        public static Color WinScreenGrass = Color.FromArgb(22, 110, 22);

        public static Color DrawScreenTop = Color.FromArgb(39, 39, 37);
        public static Color DrawScreenBottom = Color.FromArgb(185, 189, 186);

        public static Color TitleScreenBlue = Color.FromArgb(0, 53, 215);
    }
}
