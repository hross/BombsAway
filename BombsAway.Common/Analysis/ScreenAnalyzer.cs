using BombsAway.Common.Framework;
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
    public class ScreenAnalyzer : IDisposable
    {
        #region log4net
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private long _frameNumber;
        private List<StatefulScreen> _screens = new List<StatefulScreen>();

        private GameData _gameData;
        private List<PlayerColor> _colors;
        private DatabaseContext _context;
    
        public ScreenAnalyzer(List<PlayerColor> colors, DatabaseContext context)
        {
            _context = context;
            _frameNumber = 0;

            _colors = colors;
            _gameData = new GameData(_colors);

            // set up delegates
            _gameData.AddColor = this.AddColor;
            _gameData.AddGame = this.AddGame;
            _gameData.AddPlayer = this.AddPlayer;
            _gameData.AddTrophy = this.AddTrophy;
            _gameData.AddWin = this.AddWin;
            _gameData.GetColors = this.Colors;
            _gameData.AddDraw = this.AddDraw;
            _gameData.ProfileFromColor = this.ProfileFromColor;

            _screens.Add(new StatefulScreen(new TrophyScreen()));
            _screens.Add(new StatefulScreen(new GameplayScreen()));
            _screens.Add(new StatefulScreen(new PlayerSelectScreen()));
            _screens.Add(new StatefulScreen(new DrawScreen()));
            _screens.Add(new StatefulScreen(new WinScreen()));
            _screens.Add(new StatefulScreen(new TitleScreen()));
        }

        #region Add Data Delegates

        public Win AddWin(Win win)
        {
            var service = new GameWinService(_context);
            return service.Add(win);
        }

        public Draw AddDraw(Draw draw)
        {
            var service = new DrawService(_context);
            return service.Add(draw);
        }

        public Game AddGame(Game game)
        {
            var service = new GameService(_context);
            return service.Add(game);
        }

        public GamePlayer AddPlayer(GamePlayer player)
        {
            var service = new GamePlayerService(_context);
            return service.Add(player);
        }

        public PlayerColor AddColor(PlayerColor color)
        {
            var service = new PlayerColorService(_context);
            return service.Add(color);
        }

        public Trophy AddTrophy(Trophy trophy)
        {
            var service = new TrophyService(_context);
            return service.Add(trophy);
        }

        public List<PlayerColor> Colors()
        {
            var service = new PlayerColorService(_context);
            return service.QueryAll();
        }

        public Profile ProfileFromColor(PlayerColor color)
        {
            var service = new ProfileService(_context);
            return service.First(p => p.PlayerColorId == color.Id);
        }
        #endregion

        /// <summary>
        /// Returns true if any frame was processed.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public ProcessResult AnalyzeFrame(Bitmap bitmap)
        {
            log.InfoFormat("Frame # {0}", _frameNumber);

            var result = new ProcessResult();

            foreach (var screen in _screens)
            {
                var res = screen.ProcessFrame(_frameNumber, bitmap, _gameData);
                result.Reset = result.Reset || res.Reset;
                result.WasProcessed = result.WasProcessed || res.WasProcessed;
            }
            _frameNumber++;

            // if one of the screens reset our state
            if (result.Reset)
            {
                Reset();
            }

            return result;
        }

        public void Reset()
        {
            _frameNumber = 0;

            foreach (var screen in _screens)
            {
                screen.Reset();
            }

            // reset game state
            _gameData.Reset();
        }

        public void Dispose()
        {
            if (null != _screens)
            {
                foreach (var screen in _screens)
                {
                    screen.Dispose();
                }
                _screens = null;
            }
        }
    }
}
