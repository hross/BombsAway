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
    public class PlayerSelectScreen : ScreenBase
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public override string Name { get { return "playerselect"; } }

        public override int ParseDelayInFrames
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// This screen should reset the game state.
        /// </summary>
        public override bool CauseReset
        {
            get
            {
                return true;
            }
        }

        public PlayerSelectScreen(string fileName = null) : base(fileName) { }

        public PlayerSelectScreen(Bitmap frame) : base(frame) { }

        protected override bool CheckForFrameMatch()
        {
            if (this.IsGameplayScreen())
                return false;

            // create filter to find only the yellow player text color
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(ScreenData.PlayerTextColor);
            filter.Radius = 50;

            // apply the filter    
            var image = filter.Apply(this.UnmanagedFrame);

            if (log.IsDebugEnabled)
            {
                this.SaveImage(image, "selectmatch.euclidfilter");
            }

            var blobCounter = new BlobCounter();
            blobCounter.MinWidth = 5;
            blobCounter.MaxWidth = 70;
            blobCounter.MinHeight = 15;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(image);
            var rects = blobCounter.GetObjectsRectangles();

            if (log.IsDebugEnabled)
            {
                this.SaveImageBlobs(this.Frame, "selectmatch.blobs", rects);
            }

            image.Dispose();

            if (rects.Count() < 15)
            { 
                // we should have 28 letters but the detection isn't perfect
                return false;
            }

            // create filter to find only the yellow player text color
            filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(ScreenData.PlayerManColor);
            filter.Radius = 50;

            // apply the filter    
            image = filter.Apply(this.UnmanagedFrame);

            if (log.IsDebugEnabled)
            {
                this.SaveImage(image, "selectmatch.euclidfilter");
            }

            blobCounter = new BlobCounter();
            blobCounter.MinWidth = 5;
            blobCounter.MaxWidth = 70;
            blobCounter.MinHeight = 15;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(image);
            rects = blobCounter.GetObjectsRectangles();

            if (log.IsDebugEnabled)
            {
                this.SaveImageBlobs(this.Frame, "selectmatch.blobs", rects);
            }

            image.Dispose();

            if (rects.Count() > 3)
            {
                // used for debugging
                return true;
            }
            return false;
        }

        private List<int> FindPlayers(Color color)
        {
            bool[] players = new bool[4];

            // create filter to find only the com color
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(color);
            filter.Radius = 30;
            filter.FillOutside = true;

            // apply the filter    
            var image = filter.Apply(this.UnmanagedFrame);

            if (log.IsDebugEnabled)
            {
                this.SaveImage(image, "selectmatch.euclidfilter");
            }

            var blobCounter = new BlobCounter();
            blobCounter.MinWidth = 3;
            blobCounter.MaxWidth = 70;
            blobCounter.MinHeight = 3;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(image);
            var rects = blobCounter.GetObjectsRectangles();

            if (log.IsDebugEnabled)
            {
                this.SaveImageBlobs(this.Frame, "selectmatch.blobs", rects);
            }

            // check for the right lettering
            foreach (var rect in rects)
            {
                if (rect.X > 472 && rect.X < 621)
                {
                    // if this is in the right section 
                    // we check y coordinates for player
                    if (rect.Y > 66 && rect.Y < 122)
                    {
                        players[0] = true;
                    }
                    else if (rect.Y > 130 && rect.Y < 185)
                    {
                        players[1] = true;
                    }
                    else if (rect.Y > 194 && rect.Y < 250)
                    {
                        players[2] = true;
                    }
                    else if (rect.Y > 259 && rect.Y < 316)
                    {
                        players[3] = true;
                    }
                }
            }

            image.Dispose();

            var temp = new List<int>();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i])
                    temp.Add(i + 1);
            }

            return temp;
        }

        public override void AnalyzeFrame(long frameNumber, GameData game)
        {
            base.AnalyzeFrame(frameNumber, game);

            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Player select screen detected.");
            }

            game.Computers = FindPlayers(ScreenData.PlayerTextComColor);
            game.Off = FindPlayers(ScreenData.PlayerTextOffColor);
        }
    }
}
