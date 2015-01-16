using DirectShowLib;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BombsAway.Common.Capture
{
    public class CaptureVideoToBitmapAdapter : CaptureAdapterBase
    {
        private string _baseDirectoryPath;
        private string _sourceVideoPath;

        private IGraphBuilder _graphBuilder;
        private IMediaControl _mediaControl;
        private IBaseFilter _fileFilter;

        public CaptureVideoToBitmapAdapter(string sourceVideoPath, string baseDirectoryPath = null)
        {
            OnFrame = null;
            _baseDirectoryPath = baseDirectoryPath;
            _sourceVideoPath = sourceVideoPath;
        }

        public override void Start(int channel = 3)
        {
            // create the graph
            _graphBuilder = (IGraphBuilder)new FilterGraph();

            // reference to media control so we can control recording
            _mediaControl = (IMediaControl)_graphBuilder;

            // Add the video source
            int hr = _graphBuilder.AddSourceFilter(_sourceVideoPath, "file source filter", out _fileFilter);
            DsError.ThrowExceptionForHR(hr);

            // create the sample grabber and event sink
            var sampleHandler = new BitmapSampleGrabber(_baseDirectoryPath);
            sampleHandler.OnFrame = this.OnFrame;
            ISampleGrabber sampleGrabber = sampleHandler.CreateSampleGrabber();
            IBaseFilter grabFilter = (IBaseFilter)sampleGrabber;
            hr = _graphBuilder.AddFilter(grabFilter, "SampleGrabber");

            // ---------------------------------
            // Connect the file filter to the sample grabber

            // Hopefully this will be the video pin, we could check by reading it's mediatype
            IPin iPinOut = DsFindPin.ByDirection(_fileFilter, PinDirection.Output, 0);

            // Get the input pin from the sample grabber
            IPin iPinIn = DsFindPin.ByDirection(grabFilter, PinDirection.Input, 0);

            hr = _graphBuilder.Connect(iPinOut, iPinIn);
            DsError.ThrowExceptionForHR(hr);

            // this sucker prevents us from having to render to the screen
            IBaseFilter nullRender = (IBaseFilter)new NullRenderer();
            hr = _graphBuilder.AddFilter(nullRender, "null renderer");

            // ---------------------------------
            // Connect the sample grabber to the null renderer

            iPinOut = DsFindPin.ByDirection(grabFilter, PinDirection.Output, 0);
            iPinIn = DsFindPin.ByDirection(nullRender, PinDirection.Input, 0);

            hr = _graphBuilder.Connect(iPinOut, iPinIn);
            DsError.ThrowExceptionForHR(hr);

            if (this.DisableClock)
            {
                var mediaFilt = (IMediaFilter)_graphBuilder;
                hr = mediaFilt.SetSyncSource(null);
                DsError.ThrowExceptionForHR(hr);
            }

            // set up bitmap size info now that we have a video stream
            sampleHandler.SaveSizeInfo(sampleGrabber);
            
            Marshal.ReleaseComObject(sampleGrabber);
            sampleGrabber = null;
            grabFilter = null;

            hr = _mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);
        }

        public override void Stop()
        {
            if (null != _mediaControl)
            {
                _mediaControl.Stop();
                Marshal.ReleaseComObject(_graphBuilder);
                _graphBuilder = null;
                _mediaControl = null;
            }

            if (null != _fileFilter)
            {
                Marshal.ReleaseComObject(_fileFilter);
                _fileFilter = null;
            }
        }
    }
}
