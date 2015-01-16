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
    public class GameplayScreen : ScreenBase
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public override string Name { get { return "gameplay"; } }

        public GameplayScreen(string fileName = null) : base(fileName) { }

        public GameplayScreen(Bitmap frame) : base(frame) { }

        protected override bool CheckForFrameMatch()
        {
            return this.IsGameplayScreen();
        }

        public override void AnalyzeFrame(long frameNumber, GameData game)
        {
            base.AnalyzeFrame(frameNumber, game);
        }
    }
}
