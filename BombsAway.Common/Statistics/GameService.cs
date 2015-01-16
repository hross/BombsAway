using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BombsAway.Common.Framework;
using System.Linq.Expressions;

namespace BombsAway.Common.Statistics
{
    public class GameService : ServiceBase<Game, long>
    {
        public GameService(DatabaseContext context) : base(context) { }

        protected override Expression<Func<Game, bool>> DefaultSelector(long id)
        {
            return item => item.Id == id;
        }
    }
}
