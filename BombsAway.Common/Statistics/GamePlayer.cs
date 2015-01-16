using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BombsAway.Common.Statistics
{
    public class GamePlayer
    {
        public GamePlayer()
        {
            this.CreatedOnUTC = DateTime.UtcNow;
            this.QueryColor = p => null;
            this.QueryGames = p => 0;
            this.QueryTrophies = p => 0;
            this.QueryWins = p => 0;
            this.IsCom = false;
            this.IsOff = false;
        }

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public int Position { get; set; }

        public long PlayerColorId { get; set; }

        public long? ProfileId { get; set; }

        public long GameId { get; set; }

        public DateTime CreatedOnUTC { get; set; }

        public bool IsCom { get; set; }

        public bool IsOff { get; set; }

        #region Lazy Loaded Data

        private int _wins = -1;

        [Ignore]
        public int Wins
        {
            get
            {
                if (-1 == _wins)
                    _wins = this.QueryWins(this);

                return _wins;
            }
        }

        private int _games = -1;

        [Ignore]
        public int Games
        {
            get
            {
                if (-1 == _games)
                    _games = this.QueryGames(this);

                return _games;
            }
        }

        private int _trophies = -1;

        [Ignore]
        public int Trophies
        {
            get
            {
                if (-1 == _trophies)
                    _trophies = this.QueryTrophies(this);

                return _trophies;
            }
        }

        private PlayerColor _color = null;

        [Ignore]
        public PlayerColor Color
        {
            get
            {
                if (null == _color)
                    _color = this.QueryColor(this);

                return _color;
            }
        }

        #endregion

        #region Settable Delegates

        internal Func<GamePlayer, int> QueryWins { get; set; }

        internal Func<GamePlayer, int> QueryGames { get; set; }

        internal Func<GamePlayer, int> QueryTrophies { get; set; }

        internal Func<GamePlayer, PlayerColor> QueryColor { get; set; }

        #endregion

    }
}
