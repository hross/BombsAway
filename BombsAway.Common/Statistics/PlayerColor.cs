using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Statistics
{
    public class PlayerColor
    {
        public PlayerColor()
        {
            this.Name = "";
        }

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }

        public int Red { get; set; }

        public int Green { get; set; }

        public int Blue { get; set; }

        public Color FindColor()
        {
            return Color.FromArgb(this.Red, this.Green, this.Blue);
        }

        public bool IsSimilarTo(Color color)
        {
            return this.FindColor().IsSimilarTo(color);
        }
    }
}
