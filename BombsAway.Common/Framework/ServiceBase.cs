using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BombsAway.Common.Framework
{
    public abstract class ServiceBase<T, TId> : InitializableBase
        where T : new()
    {

        /// <summary>
        /// We no longer want to run our table creation scripts by default.
        /// Possibly we trigger this somewhere in settings.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="initialize"></param>
        public ServiceBase(DatabaseContext context, bool initialize = true) : base(context)
        {
            if (initialize)
            {
                Initialize();
            }
        }

        internal override void Initialize()
        {
            using (this.Context)
            {
                this.Context.Db.CreateTable<T>(overwrite: false);
            }
        }

        internal override void Destroy()
        {
            using (this.Context)
            {
                this.Context.Db.DropTable<T>();
            }
        }

        /// <summary>
        /// This is the most important method in the service. It tells the service how to select
        /// individual items by Id for CRUD operations.
        /// </summary>
        /// <param name="salesOrderIdlesOrderIdlesOrderId"></param>
        /// <returns></returns>
        protected abstract Expression<Func<T, bool>> DefaultSelector(TId id);

        #region Query Multiple

        public virtual List<T> Select(Expression<Func<T, bool>> predicate)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                return db.Select<T>(predicate);
            }
        }

        public virtual bool Exists(Expression<Func<T, bool>> predicate)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                try
                {
                    return db.Exists<T>(predicate);
                }
                catch (NotImplementedException)
                {
                    return db.Select<T>(predicate).Count() > 0;
                }
            }
        }

        public virtual List<T> QueryAll()
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                return db.Select<T>();
            }
        }

        public virtual T First(Expression<Func<T,bool>> predicate)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                return db.FirstOrDefault<T>(predicate);
            }
        }

        public virtual int Count(Expression<Func<T, bool>> expression = null)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                if (null == expression)
                    return (int)db.Count<T>();

                return (int)db.Count(expression);
            }
        }

        public virtual bool TableExists()
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;
                var type = typeof(T);

                var modelAliasAttr = type.FirstAttribute<AliasAttribute>();
                string name = modelAliasAttr != null ? modelAliasAttr.Name : type.Name;
                return db.TableExists(name);
            }
        }

        #endregion

        public virtual T QuerySingle(TId id)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                return db.Where<T>(DefaultSelector(id)).FirstOrDefault();
            }
        }

        #region Add/Update/Delete

        public virtual T Add(T item)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                db.Insert<T>(item);

                var type = item.GetType();
                if (null != type.GetProperty("Id"))
                {
                    long id = db.GetLastInsertId();
                    type.InvokeMember("Id",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                        Type.DefaultBinder, item, new object[] { id });
                }

                return item;
            }
        }

        public virtual T Update(T item)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                db.Update<T>(item);
                return item;
            }
        }


        public virtual void UpdateOnly<TKey>(T item, Expression<Func<T, TKey>> onlyFields, Expression<Func<T, bool>> where = null)
        {
            using (this.Context)
            {
                this.Context.Db.UpdateOnly(item, onlyFields: onlyFields, where: where);
            }
        }

        public virtual bool Delete(T item)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                db.Delete<T>(item);
                return true;
            }
        }

        public virtual bool DeleteById(TId id)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                db.Delete<T>(DefaultSelector(id));
                return true;
            }
        }

        public virtual int Delete(Expression<Func<T, bool>> predicate)
        {
            using (this.Context)
            {
                IDbConnection db = this.Context.Db;

                return db.Delete<T>(predicate);
            }
        }

        #endregion

        public virtual void Validate()
        {
            // this should be overriden for validation
            //TODO: validate before save or update? possibly as an option?
        }
    }
}
