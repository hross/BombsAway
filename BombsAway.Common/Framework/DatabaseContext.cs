using ServiceStack.DataAccess;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsAway.Common.Framework
{
    public class DatabaseContext : IDisposable
    {
        #region log4net
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private IDbConnection _connection;
        private IDbTransaction _transaction;

        private string _connectionString;
        private OrmLiteConnectionFactory _factory;

        /// <summary>
        /// Mini profiler should be disabled if you are not using it in an MVC project.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="type"></param>
        /// <param name="enableMiniProfiler"></param>
        public DatabaseContext(string connectionString, string providerName, Func<DbConnection, IDbConnection> wrapConnection = null)
        {
            var provider = DialectProviderFromProviderName(providerName);

            Func<IDbConnection, IDbConnection> connFilter = conn => conn;

            if (null != wrapConnection)
            {
                connFilter = conn =>
                {
                    var innerConn = conn as DbConnection;

                    if (null == innerConn)
                    {
                        innerConn = (conn as IHasDbConnection).DbConnection as DbConnection;
                    }

                    return wrapConnection(innerConn);
                };
            }
            
            this._connectionString = connectionString;
            this._factory = new OrmLiteConnectionFactory(_connectionString, provider) {
                ConnectionFilter = connFilter
            };
        }

        private static IOrmLiteDialectProvider DialectProviderFromProviderName(string providerName)
        {
            if (providerName == "System.Data.SqlLite")
            {
                return SqliteDialect.Provider;
            }
            else
            {
                return SqlServerDialect.Provider;
            }
        }

        #region Transaction Management

        public IDbTransaction OpenTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted)
        {
            if (null != this._transaction)
                return this._transaction;

            this._transaction = Db.OpenTransaction();
            return this._transaction;
        }

        public void Rollback()
        {
            if (null != this._transaction)
            {
                this._transaction.Rollback();
                this._transaction = null;
            }
        }

        public void Commit()
        {
            if (null != this._transaction)
            {
                this._transaction.Commit();
                this._transaction = null;
            }
        }

        #endregion

        #region Database and Factory

        public OrmLiteConnectionFactory Factory
        {
            get
            {
                return this._factory;
            }
        }

        /// <summary>
        /// Use this to access the raw database connection.
        /// </summary>
        public IDbConnection Db
        {
            get
            {
                if (null != _connection)
                    return _connection;

                this._connection = _factory.OpenDbConnection();

                return this._connection;
            }
        }
        
        #endregion

        #region Transaction Helper(s)

        public void AsTransaction(Action action)
        {
            // if we are already in a transaction, don't open another one
            if (null != this._transaction)
            {
                action();
                return;
            }

            using (IDbTransaction transaction = this.OpenTransaction())
            {
                try
                {
                    action();
                    this.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    this.Rollback();
                    throw;
                }
            }
        }

        public T AsTransaction<T>(Func<T> action)
        {
            // if we are already in a transaction, don't open another one
            if (null != this._transaction)
            {
                return action();
            }

            using (IDbTransaction transaction = this.OpenTransaction())
            {
                try
                {
                    var t = action();
                    this.Commit();
                    return t;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    this.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// We only dispose an open connection if there is no current transaction on it.
        /// 
        /// NOTE: this could cause a connection leak if transactions are not properly handled. possibly we can try to track this.
        /// </summary>
        public void Dispose()
        {
            if (null == _transaction && this._connection != null)
            {
                this._connection.Close();
                this._connection = null;
            }
        }

        #endregion
    }
}
