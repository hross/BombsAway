using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Statistics
{
    public class Win
    {
        public Win()
        {
            this.CreatedOnUTC = DateTime.UtcNow;
            this.RoundWins = 0;
        }

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public long GameId { get; set; }

        public long GamePlayerId { get; set; }

        public int RoundWins { get; set; }

        public int Position { get; set; }

        public DateTime CreatedOnUTC { get; set; }
    }
}
