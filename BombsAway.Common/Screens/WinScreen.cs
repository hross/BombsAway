using AForge.Imaging;
using AForge.Imaging.Filters;
using BombsAway.Common.Statistics;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Screens
{
    public class WinScreen : ScreenBase
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public override string Name { get { return "win"; } }

        public override int ParseDelayInFrames { get { return 500; } }

        public WinScreen(string fileName = null) : base(fileName) { }

        public WinScreen(Bitmap frame) : base(frame) { }

        protected override bool CheckForFrameMatch()
        {
            if (this.IsGameplayScreen())
                return false;

            // crop the very bottom middle to look for the wall
            Crop crop = new Crop(new Rectangle(0, 427, this.Frame.Width, 30));
            var source2 = crop.Apply(this.Frame);

            // create filter to remove background gray
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(Color.FromArgb(ScreenData.WinScreenGrass.R, ScreenData.WinScreenGrass.G, ScreenData.WinScreenGrass.B));
            filter.Radius = 30;
            filter.FillOutside = true;
            filter.ApplyInPlace(source2);

            var blobCounter = new BlobCounter();
            blobCounter.MinWidth = 400;
            blobCounter.MinHeight = 20;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(source2);

            // we should get one big blob if this is the victory screen
            var rects = blobCounter.GetObjectsRectangles();

            if (log.IsDebugEnabled)
            {
                var bmp = (Bitmap)source2.Clone();
                this.SaveImageBlobs(bmp, "win.euclidfilter", rects);
                bmp.Dispose();
            }

            source2.Dispose();

            return rects.Count() == 1;
        }

        public override void AnalyzeFrame(long frameNumber, GameData game)
        {
            base.AnalyzeFrame(frameNumber, game);

            var winner = FindWinner(this.Frame, game);

            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Final winner determined to be: {0}.", winner);
            }

            game.WinnerFound(winner);
        }
        
        private int FindWinner(Bitmap source, GameData game)
        {
            var group = game.Trophies.GroupBy(p => p.GamePlayerId).OrderByDescending(g => g.Count());

            var gp = group.FirstOrDefault();

            if (gp == null)
                return 0;

            var player = gp.FirstOrDefault();

            if (null == player)
                return 0;

            return player.Position;
        }
    }
}
