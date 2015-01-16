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
    public class TrophyScreen : ScreenBase
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public override string Name { get { return "trophy"; } }

        public override int ParseDelayInFrames { get { return 300; } }

        public TrophyScreen(string fileName = null) : base(fileName) { }

        public TrophyScreen(Bitmap frame) : base(frame) { }

        protected override bool CheckForFrameMatch()
        {
            if (this.IsGameplayScreen())
                return false;

            // create filter to find only the background green
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(Color.FromArgb(ScreenData.BackgroundGreen.R, ScreenData.BackgroundGreen.G, ScreenData.BackgroundGreen.B));
            filter.Radius = 30;

            // apply the filter    
            var image = filter.Apply(this.UnmanagedFrame);

            if (log.IsDebugEnabled)
            {
                this.SaveImage(image, "trophymatch.euclidfilter");
            }

            var blobCounter = new BlobCounter();
            blobCounter.MinWidth = 50;
            blobCounter.MaxWidth = 80;
            blobCounter.MinHeight = 40;
            blobCounter.MaxHeight = 80;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(image);

            // do the left edges of these blobs match the profile window?
            var rects = blobCounter.GetObjectsRectangles().Where(t => t.X < 125);

            if (log.IsDebugEnabled)
            {
                this.SaveImageBlobs(this.Frame, "trophy.blobs", rects);
            }

            image.Dispose();

            return rects.Count() == 4; // we should have 4 profiles on this screen if it's a match
        }

        public override void AnalyzeFrame(long frameNumber, GameData game)
        {
            base.AnalyzeFrame(frameNumber, game);

            // get player colors
            var p1color = this.Frame.GetPixel(ScreenData.Player1Head.X, ScreenData.Player1Head.Y);
            var p2color = this.Frame.GetPixel(ScreenData.Player2Head.X, ScreenData.Player2Head.Y);
            var p3color = this.Frame.GetPixel(ScreenData.Player3Head.X, ScreenData.Player3Head.Y);
            var p4color = this.Frame.GetPixel(ScreenData.Player4Head.X, ScreenData.Player4Head.Y);

            // find the winner
            int winner = this.FindWinner(this.UnmanagedFrame);

            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Trophy screen found. Winner: {0}", winner);
                log.InfoFormat("Player {0} - color - R:{1}, G:{2}, B:{3}", 1, p1color.R, p1color.G, p1color.B);
                log.InfoFormat("Player {0} - color - R:{1}, G:{2}, B:{3}", 2, p2color.R, p2color.G, p2color.B);
                log.InfoFormat("Player {0} - color - R:{1}, G:{2}, B:{3}", 3, p3color.R, p3color.G, p3color.B);
                log.InfoFormat("Player {0} - color - R:{1}, G:{2}, B:{3}", 4, p4color.R, p4color.G, p4color.B);
            }

            game.PlayerFound(1, p1color);
            game.PlayerFound(2, p2color);
            game.PlayerFound(3, p3color);
            game.PlayerFound(4, p4color);

            if (winner == 0)
            {
                log.Error("Unable to find a winner!");
            }

            game.TrophyFound(winner);
        }


        private int FindWinner(UnmanagedImage source)
        {
            // crop the very bottom of the image so we can just see the feet
            Crop crop = new Crop(new Rectangle(0, 433, source.Width, source.Height - 433));
            var source2 = crop.Apply(source);

            // create filter to remove background gray
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(Color.FromArgb(ScreenData.TrophyBottomGray.R, ScreenData.TrophyBottomGray.G, ScreenData.TrophyBottomGray.B));
            filter.Radius = 40;
            filter.FillOutside = false;           
            filter.ApplyInPlace(source2);

            // go greyscale to make it easier to find blobs
            var filterGreyScale = new Grayscale(0.2125, 0.7154, 0.0721);
            source2 = filterGreyScale.Apply(source2);

            var blobCounter = new BlobCounter();
            blobCounter.MinWidth = 30;
            blobCounter.MaxWidth = 80;
            blobCounter.MinHeight = 10;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(source2);
            var rects = blobCounter.GetObjectsRectangles().Where(r => r.Y < 10);

            if (log.IsDebugEnabled)
            {
                var img = new Bitmap(source2.ToManagedImage());
                this.SaveImageBlobs(img, "footmatch.crop.euclidfilter", rects);
                img.Dispose();
            }

            if (rects.Count() == 1)
            {
                var rect = rects.First();
                // look for the feet to find the winner
                if (rect.X > 122 && rect.X < 242)
                    return 1;
                if (rect.X > 242 && rect.X < 362)
                    return 2;
                if (rect.X > 362 && rect.X < 482)
                    return 3;
                if (rect.X > 482 && rect.X < 602)
                    return 4;
            }

            source2.Dispose();

            // we did not find a winner on this screen
            // this is a bug in how we find winners
            return 0;
        }

        /// <summary>
        /// Unused trophy counting method.
        /// </summary>
        private void FindTrophies()
        {
            var image = this.UnmanagedFrame.Clone();

            // create filter to filter out darks and keep yellow
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            // set center colol and radius
            filter.CenterColor = new RGB(Color.FromArgb(255, 255, 146));
            filter.Radius = 200;

            // apply the filter
            filter.ApplyInPlace(image);

            // create another filter to remove gray
            filter = new EuclideanColorFiltering();
            // set center colol and radius
            filter.CenterColor = new RGB(Color.FromArgb(170, 172, 170));
            filter.Radius = 50;
            filter.FillOutside = false;

            // apply the filter
            filter.ApplyInPlace(image);

            // create another filter to remove white
            filter = new EuclideanColorFiltering();
            // set center colol and radius
            filter.CenterColor = new RGB(Color.FromArgb(253, 253, 251));
            filter.Radius = 50;
            filter.FillOutside = false;

            // apply the filter
            filter.ApplyInPlace(image);

            var blobCounter = new BlobCounter();
            blobCounter.MinWidth = 30;
            blobCounter.MinHeight = 5;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(image);

            var rects = blobCounter.GetObjectsRectangles().Where(r => r.Y < 300);

            if (log.IsDebugEnabled)
            {
                this.SaveImageBlobs(image, "trophy.euclidfilter", rects);
            }
            
            image.Dispose();
        }
    }
}
