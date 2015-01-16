using AForge.Imaging;
using AForge.Imaging.Filters;
using BombsAway.Common.Statistics;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Screens
{
    public class DrawScreen : ScreenBase
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public override string Name { get { return "draw"; } }

        public DrawScreen(string fileName = null) : base(fileName) { }

        public DrawScreen(Bitmap frame) : base(frame) { }

        protected override bool CheckForFrameMatch()
        {
            if (this.IsGameplayScreen())
                return false;

            // if we have two big blobs on the top and bottom
            return
                IsColorRectangle(new Rectangle(0, 0, this.Frame.Width, 125), ScreenData.DrawScreenTop)
                &&
                IsColorRectangle(new Rectangle(0, 381, this.Frame.Width, 70), ScreenData.DrawScreenBottom);
        }


        private bool IsColorRectangle(Rectangle rectangle, Color color)
        {
            // crop the very top and look for dark
            Crop crop = new Crop(rectangle);
            var source2 = crop.Apply(this.Frame);

            // create filter to remove background gray
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(color.R, color.G, color.B);
            filter.Radius = 100;
            filter.FillOutside = true;
            filter.ApplyInPlace(source2);

            var blobCounter = new BlobCounter();
            blobCounter.MinWidth = 30;
            blobCounter.MinHeight = 30;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(source2);

            // we should get one big blob
            var rects = blobCounter.GetObjectsRectangles();

            if (log.IsDebugEnabled)
            {
                var bmp = (Bitmap)source2.Clone();
                this.SaveImageBlobs(bmp, string.Format("draw.{0}.euclidfilter", color.R), rects);
                bmp.Dispose();
            }

            source2.Dispose();

            return rects.Count() == 1;
        }

        public override void AnalyzeFrame(long frameNumber, GameData game)
        {
            base.AnalyzeFrame(frameNumber, game);

            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Draw screen detected.");
            }

            game.DrawHappened();
        }
    }
}
