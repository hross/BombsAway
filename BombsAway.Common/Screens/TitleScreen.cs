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
    public class TitleScreen : ScreenBase
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public override string Name { get { return "title"; } }

        public override int ParseDelayInFrames
        {
            get
            {
                return 300;
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

        public TitleScreen(string fileName = null) : base(fileName) { }

        public TitleScreen(Bitmap frame) : base(frame) { }

        protected override bool CheckForFrameMatch()
        {
            if (this.IsGameplayScreen())
                return false;

            var color = this.Frame.GetPixel(68, 28);

            return color.IsSimilarTo(ScreenData.TitleScreenBlue);
        }

        public override void AnalyzeFrame(long frameNumber, GameData game)
        {
            base.AnalyzeFrame(frameNumber, game);

            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Title screen detected.");
            }
        }
    }
}
