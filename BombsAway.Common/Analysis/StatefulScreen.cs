using BombsAway.Common.Screens;
using BombsAway.Common.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Analysis
{
    internal class StatefulScreen : IDisposable
    {
        private ScreenBase _screen;

        private long _lastProcessed;

        public StatefulScreen(ScreenBase screen)
        {
            _screen = screen;
            _lastProcessed = 0;
        }

        /// <summary>
        /// Returns true if we should reset state after this processing loop.
        /// </summary>
        /// <param name="frameNo"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public ProcessResult ProcessFrame(long frameNo, Bitmap frame, GameData game)
        {
            var result = new ProcessResult();

            // only process a frame if there is no delay
            // or if the frame count is higher than the delay
            if (_lastProcessed == 0 || _screen.ParseDelayInFrames == 0 ||
               (_screen.ParseDelayInFrames + _lastProcessed < frameNo))
            {
                // first reset the screen with this frame
                _screen.Reset(frame);

                // if this is the screen we want, process it
                if (_screen.IsFrameAMatch)
                {
                    _screen.AnalyzeFrame(frameNo, game);
                    _lastProcessed = frameNo;

                    // if we processed we might need to reset
                    result.WasProcessed = true;
                    result.Reset = _screen.CauseReset;
                    return result;
                }
            }

            return result;
        }

        public void Reset()
        {
            _lastProcessed = 1;
        }

        public void Dispose()
        {
            if (null != this._screen)
            {
                _screen.Dispose();
                _screen = null;
            }
        }
    }
}
