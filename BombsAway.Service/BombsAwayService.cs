using System;
using System.Drawing;
using System.IO;
using BombsAway.Common.Analysis;
using BombsAway.Common.Capture;
using BombsAway.Common.Framework;
using BombsAway.Common.Statistics;
using Topshelf;
using System.Configuration;
using System.Timers;
using System.Windows.Forms;

namespace BombsAway.Service
{
    public class BombsAwayService : ServiceControl
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Timer and Sync Stuff

        public const double OneSecond = 1000;
        readonly System.Timers.Timer _timer;
        
        #endregion

        private CaptureAdapterBase _adapter;
        private ScreenAnalyzer _analyzer;

        private string _baseTempPath;
        private string _bitmapPath;

        private DatabaseContext _context;

        private PreviewForm _form;

        public BombsAwayService(PreviewForm form = null)
        {
            _form = form;

            _baseTempPath = AppDomain.CurrentDomain.BaseDirectory;
            _bitmapPath = Path.Combine(_baseTempPath, "images"); // can be used in constructors to save bitmap images

            if (!Directory.Exists(_bitmapPath))
                Directory.CreateDirectory(_bitmapPath);

            _context = AppConfig.DbContext();

            var colorService = new PlayerColorService(_context);
            var profileService = new ProfileService(_context);

            var colors = colorService.QueryAll();

            // used to analyze any bitmap frames we get back
            _analyzer = new ScreenAnalyzer(colors, _context);

            int intervalSeconds = int.Parse(ConfigurationManager.AppSettings["IntervalSeconds"] ?? "120");
            _timer = new System.Timers.Timer(OneSecond * intervalSeconds) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => this.OnTimer();

        }

        #region Start and Stop

        public bool Start(HostControl hostControl)
        {
            log.Info("Service starting...");

            _timer.Start();

            Capture(_form);

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            log.Info("Service stopping...");

            _timer.Stop();

            if (null != _adapter)
            {
                _adapter.Stop();
                _adapter.Dispose();
            }

            if (null != _context)
            {
                _context.Dispose();
            }

            return true;
        }

        #endregion

        public void Capture(PreviewForm form = null)
        {
            try
            {
                _lastStartTime = DateTime.UtcNow;
                _isRunning = true;

                // output name for sample avi
                string aviPath = Path.Combine(_baseTempPath, "Assets", "trophy.avi");

                // use this to capture bitmaps directly from the TV tuner
                _adapter = new CaptureToBitmapAdapter();//_bitmapPath
                _adapter.OnFrame = OnFrame;
                //_adapter.RenderPreview = true;

                // use this to capture input from the TV tuner to the specified file
                //_adapter = new CaptureToVideoAdapter(aviPath);

                //use this to capture input from Microsoft Life Cam to the specified file
                //_adapter = new CaptureLifeCamToVideoAdapter(aviPath);
                //_adapter.RenderPreview = true;

                // use this to capture input from an AVI and dump bitmaps
                //_adapter = new CaptureVideoToBitmapAdapter(aviPath); // _bitmapPath
                //_adapter.RenderPreview = true;
                //_adapter.OnFrame = OnFrame;

                // set up preview if it exists
                if (null != form)
                {
                    _adapter.PreviewPanel = form.PreviewPanel;
                    Application.DoEvents();
                    form.Show();
                }

                _adapter.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                _adapter.Stop();
                _adapter.Dispose();
                _adapter = null;
                _isRunning = false;
            }
        }

        #region Synchronized Variables

        private object _sync = new object();

        private DateTime _lastProcessTime = DateTime.UtcNow;
        private DateTime _lastStartTime = DateTime.UtcNow;

        private bool _isRunning = true;

        #endregion

        ///<summary>Handle timer callbacks for checking.</summary>
        public void OnTimer() {

            // check to see if the adapter is around
            // if not, let's start it
            lock (_sync)
            {
                if (_isRunning)
                {
                    // if we   are running, do we want to shut down?

                    // nothing in the last 10 minutes, so shut it down
                    if (DateTime.UtcNow - _lastProcessTime > new TimeSpan(0, 10, 0))
                    {
                        log.Info("No activity for 10 minutes. Shutting down the adpater.");
                        _isRunning = false;
                        _adapter.Stop();
                        _adapter.Dispose();
                        _adapter = null;
                    }
                }
                else
                {
                    if (DateTime.UtcNow - _lastStartTime > new TimeSpan(0, 3, 0))
                    {
                        log.Info("Starting up the adapter since it is not running.");
                        // if we are not running, let's start up again
                        Capture();
                    }
                }
            }
        }

        /// <summary>
        /// Handle bitmap generation on a per frame basis
        /// </summary>
        /// <param name="bmp"></param>
        public void OnFrame(Bitmap bmp)
        {
            // save the last time we processed
            // use the lock to ensure synchronous access to the screen anaylyzer
            lock (_sync)
            {
                if (_analyzer.AnalyzeFrame(bmp).WasProcessed)
                {
                    _lastProcessTime = DateTime.UtcNow;
                }
            }
            bmp.Dispose();
        }
    }
}
