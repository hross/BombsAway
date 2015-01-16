using BombsAway.Common.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace BombsAway.Common.Statistics
{
    public class ProfileService
    {
        private DatabaseContext _context;
        private ProfileRepository _repository;

        public ProfileService(DatabaseContext context)
        {
            _context = context;
            _repository = new ProfileRepository(context);
        }

        private void PopulateChildren(Profile profile)
        {
            if (null != profile)
            {
                profile.QueryTrophies = this.Trophies;
                profile.QueryWins = this.Wins;
                profile.QueryGames = this.Games;
                profile.QueryColor = this.Color;
            }
        }

        #region Delegate implementations

        private int Wins(Profile profile)
        {
            var jn = new JoinSqlBuilder<Win, Win>();

            jn.LeftJoin<Win, GamePlayer>(w => w.GamePlayerId, p => p.Id)
                .Where<GamePlayer>(gp => gp.ProfileId == profile.Id)
                .SelectCount<Win>(w => w.Id);

            var sql = jn.ToSql();
                
            return _context.Db.GetScalar<int>(sql);
        }

        private int Games(Profile profile)
        {
            var jn = new JoinSqlBuilder<Game, Game>();

            jn.LeftJoin<Game, GamePlayer>(g => g.Id, p => p.GameId)
                .Where<GamePlayer>(gp => gp.ProfileId == profile.Id)
                .SelectCount<Game>(g => g.Id);

            var sql = jn.ToSql();

            return _context.Db.GetScalar<int>(sql);
        }

        private int Trophies(Profile profile)
        {
            var jn = new JoinSqlBuilder<Trophy, Trophy>();

            jn.LeftJoin<Trophy, GamePlayer>(t => t.GamePlayerId, p => p.Id)
                .Where<GamePlayer>(gp => gp.ProfileId == profile.Id)
                .SelectCount<Trophy>(t => t.Id);

            var sql = jn.ToSql();

            return _context.Db.GetScalar<int>(sql);
        }

        private PlayerColor Color(Profile profile)
        {
            var playerColor = _context.Db.FirstOrDefault<PlayerColor>(pc => pc.Id == profile.PlayerColorId);

            if (null != playerColor)
                return playerColor;

            return new PlayerColor();
        }

        #endregion

        #region Repository Passthroughs

        public virtual List<Profile> Select(Expression<Func<Profile, bool>> predicate)
        {
            var items = _repository.Select(predicate);

            foreach (var item in items)
            {
                PopulateChildren(item);
            }

            return items;
        }

        public virtual List<Profile> QueryAll()
        {
            var items = _repository.QueryAll();

            foreach (var item in items)
            {
                PopulateChildren(item);
            }

            return items;
        }

        public virtual Profile First(Expression<Func<Profile, bool>> predicate)
        {
            var item = _repository.First(predicate);
            PopulateChildren(item);
            return item;
        }

        public virtual Profile QuerySingle(long id)
        {
            var item = _repository.QuerySingle(id);
            PopulateChildren(item);
            return item;
        }

        public virtual Profile Add(Profile item)
        {
            // reset anyone who has this color already
            _repository.UpdateOnly(new Profile { PlayerColorId = 1 }, p => p.PlayerColorId, p => p.PlayerColorId == item.PlayerColorId);
            
            item = _repository.Add(item);
            PopulateChildren(item);
            return item;
        }

        public virtual Profile Update(Profile item)
        {
            // reset anyone who has this color already
            _repository.UpdateOnly(new Profile { PlayerColorId = 1 }, p => p.PlayerColorId, p => p.PlayerColorId == item.PlayerColorId);
            
            item = _repository.Update(item);
            this.PopulateChildren(item);
            return item;
        }

        #endregion

        private class ProfileRepository : ServiceBase<Profile, long>
        {
            public ProfileRepository(DatabaseContext context) : base(context) { }

            internal override void Initialize()
            {
                base.Initialize();
                this.Context.Db.CreateTable<Trophy>(overwrite: false);
                this.Context.Db.CreateTable<Game>(overwrite: false);
                this.Context.Db.CreateTable<Win>(overwrite: false);
                this.Context.Db.CreateTable<GamePlayer>(overwrite: false);
                this.Context.Db.CreateTable<Draw>(overwrite: false);
            }

            protected override Expression<Func<Profile, bool>> DefaultSelector(long id)
            {
                return item => item.Id == id;
            }
        }
    }
}
