using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Statistics
{
    public class Game
    {
        public Game()
        {
            this.CreatedOnUTC = DateTime.UtcNow;
        }

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public DateTime CreatedOnUTC { get; set; }
    }
}
