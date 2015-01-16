using System;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace BombsAway.Common.Capture
{
    public class CaptureLifeCamToVideoAdapter : CaptureAdapterBase
    {
        private string _fileName;

        private IGraphBuilder _graphBuilder;
        private IMediaControl _mediaControl;
        private IBaseFilter _device;
        private IAMCrossbar _cam;
        private string _compressorName;
        private DateTime startTime;

        public CaptureLifeCamToVideoAdapter(string fileName = null, string compressorName = "ffdshow")
        {
            _fileName = fileName;
            _compressorName = compressorName;
        }


        public override void Start(int channel = 3)
        {
            int hr;

            startTime = DateTime.Now;

            _device = FindVideoDevice("Micro");

            // create the graph
            _graphBuilder = (IGraphBuilder)new FilterGraph();

            // create the graph builder
            ICaptureGraphBuilder2 captureGraphBuilder = null;
            captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            // reference to media control so we can control recording
            _mediaControl = (IMediaControl)_graphBuilder;

            // attach the filter graph to the capture graph
            hr = captureGraphBuilder.SetFiltergraph(_graphBuilder);
            DsError.ThrowExceptionForHR(hr);

            // add video input device as the source for our recording
            hr = _graphBuilder.AddFilter(_device, "source filter");
            DsError.ThrowExceptionForHR(hr);

            // add a video compressor so its not too giant
            var compressor = this.FindCompressor(new[] { _compressorName, "ffdshow", "MJPEG", "DV Video" });//MJPEG //"DV Video"
            if (compressor != null)
            {
                hr = _graphBuilder.AddFilter(compressor, "compressor filter");
                DsError.ThrowExceptionForHR(hr);
            }

            IBaseFilter mux = null;
            IFileSinkFilter sink = null;
            if (!string.IsNullOrEmpty(this._fileName))
            {
                _fileName = _fileName.Substring(0, _fileName.Length - 4) + "-" + _compressorName + ".avi";

                // create the file writer part of the graph. SetOutputFileName does this for us, and returns the mux and sink
                hr = captureGraphBuilder.SetOutputFileName(MediaSubType.Avi, _fileName, out mux, out sink);
                DsError.ThrowExceptionForHR(hr);

                // connect the device and compressor to the mux to render the capture part of the graph
                hr = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, _device, compressor, mux);
                DsError.ThrowExceptionForHR(hr);
            }

            this.RenderPreview(_device, _graphBuilder, captureGraphBuilder);

            object o;
            hr = captureGraphBuilder.FindInterface(null, null, _device, typeof(IAMCrossbar).GUID, out o);
            if (hr >= 0)
            {
                _cam = (IAMCrossbar)o;
                o = null;
            }

            if (!string.IsNullOrEmpty(this._fileName))
            {
                Marshal.ReleaseComObject(mux);
                mux = null;
                Marshal.ReleaseComObject(sink);
                sink = null;
            }

            if (null != compressor)
            {
                Marshal.ReleaseComObject(compressor);
                compressor = null;
            }

            Marshal.ReleaseComObject(captureGraphBuilder);
            captureGraphBuilder = null;

            hr = _mediaControl.Run();
            DsError.ThrowExceptionForHR(hr);
        }

        public override void Stop()
        {
            if (null != _cam)
            {
                Marshal.ReleaseComObject(_cam);
                _cam = null;
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
