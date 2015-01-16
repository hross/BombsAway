using BombsAway.Common.Framework;
using ServiceStack.OrmLite;
using System;
using System.Linq.Expressions;

namespace BombsAway.Common.Statistics
{
    public class PlayerColorService : ServiceBase<PlayerColor, long>
    {
        public PlayerColorService(DatabaseContext context) : base(context) { }

        internal override void Initialize()
        {
            base.Initialize();

            if (this.Context.Db.Count<PlayerColor>() == 0)
            {
                // default color that is not on a bomberman
                this.Context.Db.Insert(new PlayerColor { Red = 0, Green = 0, Blue = 0, Name = "None" });
                this.Context.Db.Insert(new PlayerColor { Red = 219, Green = 215, Blue = 218, Name = "White" });
                this.Context.Db.Insert(new PlayerColor { Red = 46, Green = 41, Blue = 51, Name = "Black" });
                this.Context.Db.Insert(new PlayerColor { Red = 198, Green = 27, Blue = 6, Name = "Red" });
                this.Context.Db.Insert(new PlayerColor { Red = 0, Green = 53, Blue = 207, Name = "Blue" });
                this.Context.Db.Insert(new PlayerColor { Red = 218, Green = 105, Blue = 0, Name = "Orange" });
                this.Context.Db.Insert(new PlayerColor { Red = 138, Green = 58, Blue = 93, Name = "Light Purple" });
                this.Context.Db.Insert(new PlayerColor { Red = 12, Green = 104, Blue = 55, Name = "Dark Green" });
                this.Context.Db.Insert(new PlayerColor { Red = 249, Green = 69, Blue = 143, Name = "Pink" });
                this.Context.Db.Insert(new PlayerColor { Red = 139, Green = 138, Blue = 147, Name = "Gray" });
                this.Context.Db.Insert(new PlayerColor { Red = 126, Green = 60, Blue = 12, Name = "Brown" });
                this.Context.Db.Insert(new PlayerColor { Red = 88, Green = 179, Blue = 49, Name = "Light Green" });
                this.Context.Db.Insert(new PlayerColor { Red = 116, Green = 0, Blue = 121, Name = "Dark Purple" });
                this.Context.Db.Insert(new PlayerColor { Red = 210, Green = 165, Blue = 0, Name = "Yellow" });
                this.Context.Db.Insert(new PlayerColor { Red = 2, Green = 0, Blue = 143, Name = "Dark Blue" });
                this.Context.Db.Insert(new PlayerColor { Red = 71, Green = 192, Blue = 214, Name = "Light Blue" });
            }
        }

        protected override Expression<Func<PlayerColor, bool>> DefaultSelector(long id)
        {
            return item => item.Id == id;
        }
    }
}
