using DirectShowLib;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BombsAway.Common.Capture
{
    public class CaptureToBitmapAdapter : CaptureAdapterBase
    {
        private string _baseDirectoryPath;

        private IGraphBuilder _graphBuilder;
        private IMediaControl _mediaControl;
        private IBaseFilter _device;
        private IAMTVTuner _tuner;

        public CaptureToBitmapAdapter(string baseDirectoryPath = null)
        {
            OnFrame = null;
            _baseDirectoryPath = baseDirectoryPath;
        }

        public override void Start(int channel = 3)
        {
            _device = FindVideoDevice("Micro");

            // create the graph
            _graphBuilder = (IGraphBuilder)new FilterGraph();

            // create the graph builder
            ICaptureGraphBuilder2 captureGraphBuilder = null;
            captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            // reference to media control so we can control recording
            _mediaControl = (IMediaControl)_graphBuilder;

            // attach the filter graph to the capture graph
            int hr = captureGraphBuilder.SetFiltergraph(_graphBuilder);
            DsError.ThrowExceptionForHR(hr);

            // add video input device as the source for our recording
            hr = _graphBuilder.AddFilter(_device, "source filter");
            DsError.ThrowExceptionForHR(hr);

            // create the sample grabber and event sink
            var sampleHandler = new BitmapSampleGrabber(_baseDirectoryPath);
            sampleHandler.OnFrame = this.OnFrame;
            ISampleGrabber sampleGrabber = sampleHandler.CreateSampleGrabber();
            hr = _graphBuilder.AddFilter((IBaseFilter)sampleGrabber, "SampleGrabber");

            // this sucker prevents us from having to render to the screen
            IBaseFilter nullRender = (IBaseFilter)new NullRenderer();
            hr = _graphBuilder.AddFilter(nullRender, "null renderer");


            hr = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, _device, (IBaseFilter)sampleGrabber, nullRender);
            DsError.ThrowExceptionForHR(hr);

            this.RenderPreview(_device, _graphBuilder, captureGraphBuilder);

            if (this.DisableClock)
            {
                var mediaFilt = (IMediaFilter)_graphBuilder;
                hr = mediaFilt.SetSyncSource(null);
                DsError.ThrowExceptionForHR(hr);
            }

            // set up bitmap size info now that we have a video stream
            sampleHandler.SaveSizeInfo(sampleGrabber);
            
            // change the channel on the tuner
            object o;
            IAMTVTuner tuner = null;
            hr = captureGraphBuilder.FindInterface(null, null, _device, typeof(IAMTVTuner).GUID, out o);
            if (hr >= 0)
            {
                tuner = (IAMTVTuner)o;
                o = null;
            }

            hr = tuner.put_Channel(channel, 0, 0); // change to channel

            Marshal.ReleaseComObject(captureGraphBuilder);
            captureGraphBuilder = null;

            Marshal.ReleaseComObject(sampleGrabber);
            sampleGrabber = null;

            hr = _mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);
        }

        public override void Stop()
        {
            if (null != _tuner)
            {
                Marshal.ReleaseComObject(_tuner);
                _tuner = null;
            }

            if (null != _mediaControl)
            {
                _mediaControl.Stop();
                Marshal.ReleaseComObject(_graphBuilder);
                _graphBuilder = null;
                _mediaControl = null;
            }

            if (null != _device)
            {
                Marshal.ReleaseComObject(_device);
                _device = null;
            }
        }
    }
}
