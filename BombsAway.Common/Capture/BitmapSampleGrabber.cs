using DirectShowLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace BombsAway.Common.Capture
{
    public class BitmapSampleGrabber : ISampleGrabberCB
    {
        private int _width = 0;
        private int _height = 0;
        private int _stride = 0;
        private int _size = 0;

        // how many frames have we captured?
        private int _frames = 0;

        private string _baseDirectoryPath;

        /// <summary>
        /// A callback that gets fired when a new bitmap is processed.
        /// </summary>
        public Action<Bitmap> OnFrame { get; set; }

        public BitmapSampleGrabber(string baseDirectoryPath = null)
        {
            this.OnFrame = null;
            this._baseDirectoryPath = baseDirectoryPath;
        }

        /// <summary>
        /// Create a sample grabber that calls back into this object.
        /// </summary>
        /// <returns></returns>
        public ISampleGrabber CreateSampleGrabber()
        {
            ISampleGrabber sampleGrabber = (ISampleGrabber)new SampleGrabber();

            // set the media type of the sample grabber
            AMMediaType media = new AMMediaType();
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.RGB24;
            media.formatPtr = IntPtr.Zero;

            int hr = sampleGrabber.SetMediaType(media);
            hr = sampleGrabber.SetBufferSamples(false);
            hr = sampleGrabber.SetOneShot(false);
            hr = sampleGrabber.SetCallback(this, 1);

            return sampleGrabber;
        }

        /// <summary>
        /// Take in the existing sample grabber,
        /// (after it has been started)
        /// read off the media format and store the size/properties of the video stream
        /// for later capture.
        /// </summary>
        /// <param name="sampGrabber"></param>
        public void SaveSizeInfo(ISampleGrabber sampGrabber)
        {
            int hr;
            // Get the media type from the SampleGrabber
            AMMediaType media = new AMMediaType();
            hr = sampGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            // Grab the size info
            VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
            _width = videoInfoHeader.BmiHeader.Width;
            _height = videoInfoHeader.BmiHeader.Height;
            _stride = _width * (videoInfoHeader.BmiHeader.BitCount / 8);
            _size = videoInfoHeader.BmiHeader.ImageSize;

            DsUtils.FreeAMMediaType(media);
            media = null;
        }
        
        /// <summary>
        /// This is the callback that gets fired for each video frame.
        /// </summary>
        /// <param name="SampleTime"></param>
        /// <param name="pBuffer"></param>
        /// <param name="BufferLen"></param>
        /// <returns></returns>
        int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            _frames++;

            //TODO: use log4net here
            Debug.WriteLine(string.Format("Frame: {0}", _frames));

            using (var bitmap = new Bitmap(_width, _height, _width * 3, PixelFormat.Format24bppRgb, pBuffer))
            {

                // output to disk if requested
                if (!string.IsNullOrEmpty(_baseDirectoryPath))
                {
                    bitmap.Save(Path.Combine(_baseDirectoryPath, string.Format("{0}.bmp", _frames)));
                }

                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

                // fire the callback if there is one
                if (this.OnFrame != null)
                {
                    this.OnFrame(bitmap);
                }
            }

            return 0;
        }
        
        /// <summary>
        /// This does nothing. It is just to implement the interface
        /// </summary>
        /// <param name="SampleTime"></param>
        /// <param name="pSample"></param>
        /// <returns></returns>
        int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
        {
            return 0;
        }
    }
}
