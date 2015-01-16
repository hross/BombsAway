using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DirectShowLib;
using System.Windows.Forms;

namespace BombsAway.Common.Capture
{
    public abstract class CaptureAdapterBase : IDisposable
    {
        /// <summary>
        /// If you want to run as fast as possible, disable clock syncing on frame capture.
        /// </summary>
        public bool DisableClock { get; set; }

        /// <summary>
        /// If you want to see a preview of what is happening.
        /// </summary>
        public Panel PreviewPanel { get; set; }

        /// <summary>
        /// A callback that gets fired when a new bitmap is processed.
        /// </summary>
        public Action<Bitmap> OnFrame { get; set; }

        public CaptureAdapterBase()
        {
            this.OnFrame = null;
            this.PreviewPanel = null;
            this.DisableClock = false;
        }

        protected IBaseFilter FindVideoDevice(string startsWith)
        {
            return this.FindDevice(startsWith, FilterCategory.VideoInputDevice);
        }

        protected IBaseFilter FindCompressor(string[] startsWith)
        {
            List<string> startsWithList = startsWith.ToList();
            while (startsWithList.Count > 0)
            {
                var compressor = this.FindDevice(startsWithList[0], FilterCategory.VideoCompressorCategory);

                if (compressor != null)
                {
                    return compressor;
                }

                startsWithList.RemoveAt(0);
            }

            return null;
        }

        protected IBaseFilter FindCompressor(string startsWith)
        {
            return this.FindDevice(startsWith, FilterCategory.VideoCompressorCategory);
        }

        private IBaseFilter FindDevice(string startsWith, Guid category)
        {
            Guid iid = typeof(IBaseFilter).GUID;
            foreach (DsDevice ds in DsDevice.GetDevicesOfCat(category))
            {
                if (ds.Name.StartsWith(startsWith))
                {
                    object source = null;
                    ds.Mon.BindToObject(null, null, ref iid, out source);
                    return (IBaseFilter)source;
                }
            }

            return null;
        }

        protected void RenderPreview(IBaseFilter device, IGraphBuilder graphBuilder, ICaptureGraphBuilder2 captureGraphBuilder)
        {
            // only do this if we actually have a preview pane
            if (null != this.PreviewPanel)
            {
                var hr = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, device, null, null);

                // get the video window from the graph
                IVideoWindow videoWindow = null;
                videoWindow = (IVideoWindow)graphBuilder;

                // set the owener of the videoWindow to an IntPtr of some sort (the Handle of any control - could be a form / button etc.)
                hr = videoWindow.put_Owner(this.PreviewPanel.Handle);
                DsError.ThrowExceptionForHR(hr);

                // set the style of the video window
                hr = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
                DsError.ThrowExceptionForHR(hr);

                // position video window in client rect of main application window
                hr = videoWindow.SetWindowPosition(0, 0, this.PreviewPanel.Width, this.PreviewPanel.Height);
                DsError.ThrowExceptionForHR(hr);

                // make the video window visible
                hr = videoWindow.put_Visible(OABool.True);
                DsError.ThrowExceptionForHR(hr);
            }
        }

        public abstract void Start(int channel = 3);

        public abstract void Stop();

        public virtual void Dispose()
        {
            this.Stop();
        }
    }
}
