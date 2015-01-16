using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BombsAway.Common.Statistics
{
    public class Profile
    {
        public Profile()
        {
            this.QueryColor = p => null;
            this.QueryGames = p => 0;
            this.QueryTrophies = p => 0;
            this.QueryWins = p => 0;

            this.CreatedOnUTC = DateTime.UtcNow;
        }

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [Display(Name = "Color")]
        public long PlayerColorId { get; set; }


        [Display(Name = "Name")]
        public string Name { get; set; }

        public DateTime CreatedOnUTC { get; set; }

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

        private PlayerColor _playerColor = null;

        [Ignore]
        public PlayerColor Color
        {
            get
            {
                if (null == _playerColor)
                    _playerColor = this.QueryColor(this);

                return _playerColor;
            }
        }

        #endregion

        #region Settable Delegates

        internal Func<Profile, int> QueryWins { get; set; }

        internal Func<Profile, int> QueryGames { get; set; }

        internal Func<Profile, int> QueryTrophies { get; set; }

        internal Func<Profile, PlayerColor> QueryColor { get; set; }

        #endregion
    }
}
