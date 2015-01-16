using BombsAway.Common.Capture;
using BombsAway.Common.Screens;
using BombsAway.Common.Statistics;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Topshelf;

namespace BombsAway.Service
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            HostFactory.Run(x =>
            {
                //new PreviewForm()
                x.Service(settings => new BombsAwayService(), s => { });
                x.RunAsLocalSystem();

                x.SetDescription("Bombs Away Service");
                x.SetDisplayName("Bombs Away");
                x.SetServiceName("BombsAway");
            });
        }
    }
}
