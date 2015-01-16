using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BombsAway.Common.Framework;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace BombsAway.Common.Statistics
{
    public class GamePlayerService
    {
        private DatabaseContext _context;
        private GamePlayerRepository _repository;

        public GamePlayerService(DatabaseContext context)
        {
            _context = context;
            _repository = new GamePlayerRepository(context);
        }

        private void PopulateChildren(GamePlayer player)
        {
            if (null != player)
            {
                player.QueryTrophies = this.Trophies;
                player.QueryWins = this.Wins;
                player.QueryGames = this.Games;
                player.QueryColor = this.Color;
            }
        }

        #region Delegate implementations

        private int Wins(GamePlayer item)
        {
            var jn = new JoinSqlBuilder<Win, Win>();

            jn.LeftJoin<Win, GamePlayer>(w => w.GamePlayerId, p => p.Id)
                .Where<GamePlayer>(gp => gp.Id == item.Id)
                .SelectCount<Win>(w => w.Id);

            var sql = jn.ToSql();

            return _context.Db.GetScalar<int>(sql);
        }

        private int Games(GamePlayer item)
        {
            var jn = new JoinSqlBuilder<Game, Game>();

            jn.LeftJoin<Game, GamePlayer>(g => g.Id, p => p.GameId)
                .Where<GamePlayer>(gp => gp.Id == item.Id)
                .SelectCount<Game>(g => g.Id);

            var sql = jn.ToSql();

            return _context.Db.GetScalar<int>(sql);
        }

        private int Trophies(GamePlayer item)
        {
            var jn = new JoinSqlBuilder<Trophy, Trophy>();

            jn.LeftJoin<Trophy, GamePlayer>(t => t.GamePlayerId, p => p.Id)
                .Where<GamePlayer>(gp => gp.Id == item.Id)
                .SelectCount<Trophy>(t => t.Id);

            var sql = jn.ToSql();

            return _context.Db.GetScalar<int>(sql);
        }

        private PlayerColor Color(GamePlayer player)
        {
            var playerColor = _context.Db.FirstOrDefault<PlayerColor>(pc => pc.Id == player.PlayerColorId);

            if (null != playerColor)
                return playerColor;

            return new PlayerColor();
        }

        #endregion

        #region Repository Passthroughs

        public virtual List<GamePlayer> Select(Expression<Func<GamePlayer, bool>> predicate)
        {
            var items = _repository.Select(predicate);

            foreach (var item in items)
            {
                PopulateChildren(item);
            }

            return items;
        }

        public virtual List<GamePlayerProfile> SelectProfiles(Expression<Func<GamePlayer, bool>> predicate)
        {
            var jn = new JoinSqlBuilder<GamePlayerProfile, GamePlayer>();

            jn.LeftJoin<GamePlayer, Profile>(
                g => g.ProfileId, p => p.Id,
                gp => new { gp.Id, gp.GameId, gp.PlayerColorId, gp.ProfileId, gp.CreatedOnUTC, gp.Position },
                p => new { p.Name })
                .Where<GamePlayer>(predicate);

            var sql = jn.ToSql();

            var items = _context.Db.Select<GamePlayerProfile>(sql);

            foreach (var item in items)
            {
                PopulateChildren(item);
            }

            return items;
        }

        public virtual GamePlayer Add(GamePlayer item)
        {
            item = _repository.Add(item);
            PopulateChildren(item);
            return item;
        }

        #endregion

        private class GamePlayerRepository : ServiceBase<GamePlayer, long>
        {
            public GamePlayerRepository(DatabaseContext context) : base(context) { }

            internal override void Initialize()
            {
                base.Initialize();
                this.Context.Db.CreateTable<Trophy>(overwrite: false);
                this.Context.Db.CreateTable<Game>(overwrite: false);
                this.Context.Db.CreateTable<Win>(overwrite: false);
                this.Context.Db.CreateTable<GamePlayer>(overwrite: false);
                this.Context.Db.CreateTable<Draw>(overwrite: false);
            }

            protected override Expression<Func<GamePlayer, bool>> DefaultSelector(long id)
            {
                return item => item.Id == id;
            }
        }
    }
}
