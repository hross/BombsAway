using AForge.Imaging;
using BombsAway.Common.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Screens
{
    public abstract class ScreenBase : IDisposable
    {
        public abstract string Name { get; }

        /// <summary>
        /// After we parse a valid frame how many frames should we ignore until we try to get another one?
        /// </summary>
        public virtual int ParseDelayInFrames { get { return 500; } }

        #region Frame Storage

        protected Bitmap Frame { get; private set; }

        private UnmanagedImage _unmanaged;
        protected UnmanagedImage UnmanagedFrame
        {
            get
            {
                if (null == _unmanaged)
                    _unmanaged = UnmanagedImage.FromManagedImage(this.Frame);

                return _unmanaged;
            }
        }

        #endregion

        public string BaseTempPath { get; set; }

        #region Constructors

        /// <summary>
        /// Reset the analyzer with a new frame to analyze.
        /// </summary>
        /// <param name="frame"></param>
        public void Reset(Bitmap frame)
        {
            if (null != this.Frame)
            {
                this.Frame.Dispose();
                this.Frame = null;
            }

            if (null != this._unmanaged)
            {
                _unmanaged.Dispose();
                _unmanaged = null;
            }

            this._isFrameAMatch = null;
            this.Frame = frame;
        }

        private ScreenBase()
        {
            this.BaseTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
        }

        public ScreenBase(string fileName) : this()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                // load a default bmp if we haven't been given one
                fileName = Path.Combine(this.BaseTempPath, this.Name + ".bmp");
            }

            if (File.Exists(fileName))
                this.Frame = new Bitmap(fileName);
        }

        public ScreenBase(Bitmap frame) : this()
        {
            Reset(frame);
        }

        #endregion

        #region Lazy Loaded Frame Matching

        private bool? _isFrameAMatch;

        /// <summary>
        /// Does this screen match?
        /// 
        /// This is cached so it only runs once per screen.
        /// </summary>
        public bool IsFrameAMatch
        {
            get
            {
                if (_isFrameAMatch.HasValue)
                    return _isFrameAMatch.Value;

                _isFrameAMatch = CheckForFrameMatch();

                return _isFrameAMatch.Value;
            }
        }

        /// <summary>
        /// This method will check to see if the frame passed into this screen
        /// actually matches the screen we are looking for.
        /// </summary>
        /// <returns></returns>
        protected abstract bool CheckForFrameMatch();

        #endregion

        /// <summary>
        /// Analyze a frame and do stuff.
        /// </summary>
        public virtual void AnalyzeFrame(long frameNumber, GameData game)
        {
            if (!this.IsFrameAMatch)
            {
                throw new Exception("Unable to analyze this frame because it does not match the expected screen type!");
            }
        }

        /// <summary>
        /// Should this frame reset the state (i.e. frame counts, etc)
        /// </summary>
        public virtual bool CauseReset
        {
            get
            {
                return false;
            }
        }

        #region Misc Helpers

        protected bool IsGameplayScreen()
        {
            var color = this.Frame.GetPixel(89, 29);

            return color.IsSimilarTo(ScreenData.PlayscreenTopColor);
        }

        #endregion

        #region Image Saving Helpers

        protected void SaveImage(Bitmap bitmap, string name)
        {
            bitmap.Save(Path.Combine(this.BaseTempPath, name + ".bmp"));
        }

        protected void SaveImage(UnmanagedImage bitmap, string name)
        {
            var bmp = bitmap.ToManagedImage();
            this.SaveImage(bmp, Path.Combine(this.BaseTempPath, name));
            bmp.Dispose();
        }

        protected void SaveImageBlobs(Bitmap image, string name, IEnumerable<Rectangle> rectangles)
        {
            Graphics g = Graphics.FromImage(image);
            foreach (Rectangle objectRect in rectangles)
            {
                using (Pen pen = new Pen(Color.FromArgb(160, 255, 160), 5))
                {
                    g.DrawRectangle(pen, objectRect);
                }
            }
            g.Dispose();

            this.SaveImage(image, name);
        }

        protected void SaveImageBlobs(UnmanagedImage image, string name, IEnumerable<Rectangle> rectangles)
        {
            var img = image.ToManagedImage();

            this.SaveImageBlobs(image, name, rectangles);

            img.Dispose();
        }

        #endregion

        public void Dispose()
        {
            this.Reset(null);
        }
    }
}
