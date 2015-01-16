using BombsAway.Common.Screens;
using BombsAway.Common.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Analyze();
        }

        /// <summary>
        /// Example of how to do a test analysis for a single screen.
        /// </summary>
        public static void Analyze()
        {
            using (var context = AppConfig.DbContext())
            {
                var colorService = new PlayerColorService(context);

                var colors = colorService.QueryAll();

                var screen = new PlayerSelectScreen();

                screen.AnalyzeFrame(0, new GameData(colors));

                screen.Dispose();
            }
        }
    }
}
