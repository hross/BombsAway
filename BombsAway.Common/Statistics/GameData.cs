using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BombsAway.Common.Statistics
{
    /// <summary>
    /// Parent class for all the parts of a game we might want to use in a screen.
    /// </summary>
    public class GameData
    {
        public GameData(List<PlayerColor> colors)
        {
            Colors = colors;

            AddPlayer = p => p;
            AddTrophy = t => t;
            AddColor = c => c;
            AddGame = g => g;
            AddWin = w => w;
            AddDraw = g => g;
            ProfileFromColor = c => null;
            GetColors = () => new List<PlayerColor>();

            Reset();
        }

        #region Stateful Objects

        public Game Game { get; set; }

        public List<GamePlayer> Players { get; set; }

        public List<Trophy> Trophies { get; set; }

        public List<Draw> Draws { get; set; }

        public List<PlayerColor> Colors { get; set; }

        public List<int> Computers { get; set; }

        public List<int> Off { get; set; }

        #endregion

        public bool Initialized
        {
            get
            {
                return this.Players.Count > 0;
            }
        }

        #region Methods that do things

        public void PlayerFound(int position, Color color) {

            // make sure we actually have colors first
            if (this.Colors.Count == 0)
            {
                this.Colors = GetColors();
            }

            // if there is no game yet, we need to add it
            if (this.Game.Id == 0)
            {
                Game = this.AddGame(this.Game);
            }

            // find the player color
            var pc = this.Colors.FirstOrDefault(c => c.IsSimilarTo(color));

            if (null == pc)
            {
                // add a player color first, since we didn't find it
                pc = new PlayerColor
                {
                    Red = color.R,
                    Green = color.G,
                    Blue = color.B
                };
                pc = this.AddColor(pc);

                this.Colors.Add(pc);
            }

            // they exist already
            if (this.Players.Any(p => p.Position == position))
                return;

            // add them and save them for later
            var gp = new GamePlayer
            {
                GameId = this.Game.Id,
                Position = position,
                PlayerColorId = pc.Id,
                IsCom = Computers.Any(i => i == position),
                IsOff = Off.Any(i => i == position)
            };

            if (!gp.IsOff && !gp.IsCom)
            {
                // find the profile associated with this color
                var profile = this.ProfileFromColor(pc);

                if (null != profile)
                    gp.ProfileId = profile.Id;
            }

            gp = AddPlayer(gp);

            this.Players.Add(gp);
        }

        public void TrophyFound(int position)
        {
            Trophy t = new Trophy
            {
                GameId = this.Game.Id,
                Position = position
            };

            var player = this.Players.FirstOrDefault(gp => gp.Position == position);

            if (player != null)
            {
                t.GamePlayerId = player.Id;
            }

            t = AddTrophy(t);

            this.Trophies.Add(t);
        }

        public void WinnerFound(int position)
        {
            Win w = new Win
            {
                GameId = this.Game.Id,
                Position = position
            };

            var player = this.Players.FirstOrDefault(gp => gp.Position == position);

            if (player != null)
            {
                w.RoundWins = this.Trophies.Count(t => t.GamePlayerId == player.Id);
                w.GamePlayerId = player.Id;
            }

            w = AddWin(w);
        }

        public void DrawHappened()
        {
            Draw d = new Draw
            {
                GameId = this.Game.Id
            };

            d = AddDraw(d);

            this.Draws.Add(d);
        }

        #endregion

        /// <summary>
        /// Delegates for game actions.
        /// </summary>

        public Func<Game, Game> AddGame { get; set; }

        public Func<GamePlayer, GamePlayer> AddPlayer { get; set; }

        public Func<Trophy, Trophy> AddTrophy { get; set; }

        public Func<PlayerColor, PlayerColor> AddColor { get; set; }

        public Func<Win, Win> AddWin { get; set; }

        public Func<Draw, Draw> AddDraw { get; set; }

        public Func<List<PlayerColor>> GetColors { get; set; }

        public Func<PlayerColor, Profile> ProfileFromColor { get; set; }

        public void Reset()
        {
            Game = new Game();
            Players = new List<GamePlayer>();
            Trophies = new List<Trophy>();
            Draws = new List<Draw>();
            Computers = new List<int>();
            Off = new List<int>();
        }
    }
}
