using BombsAway.Common.Framework;
using Newtonsoft.Json;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;

namespace BombsAway
{
    public class AppConfig
    {
        static AppConfig()
        {
            string localOverride = ConfigurationManager.AppSettings["LocalDbPath"];

            if (string.IsNullOrEmpty(localOverride))
            {
                // default to a hard coded path
                localOverride = @"C:\bombsaway.sqlite";
            }

            LocalDbPath = localOverride;
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["DataSource"];
            if (null != connectionStringSettings)
            {
                _connectionString = connectionStringSettings.ConnectionString;
                _providerName = connectionStringSettings.ProviderName;
            }
        }

        #region Database

        private static string LocalDbPath;
        private static string _connectionString;
        private static string _providerName;

        /// <summary>
        /// **ConnectionString
        /// </summary>
        public static DatabaseContext DbContext(Func<DbConnection, IDbConnection> wrapConnection = null)
        {
            return new DatabaseContext(_connectionString.Replace("[[localdb]]", LocalDbPath), _providerName, wrapConnection);
        }

        public static string ConnectionString
        {
            get
            {
                _sync.EnterReadLock();
                try
                {
                    if (null == _connectionString)
                        return null;
                    return _connectionString.Replace("[[localdb]]", LocalDbPath);
                }
                finally
                {
                    _sync.ExitReadLock();
                }
            }
            set
            {
                _sync.EnterWriteLock();
                try
                {
                    _connectionString = value;
                }
                finally
                {
                    _sync.ExitWriteLock();
                }
            }
        }

        public static bool IsSqlLite
        {
            get
            {
                var connectionString = ConnectionString;
                return !string.IsNullOrEmpty(connectionString) && !connectionString.Contains("Server=");
            }
        }

        #endregion

        #region Cache

        private static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();
        private static Dictionary<string, ApplicationConfigSetting> _cache = new Dictionary<string, ApplicationConfigSetting>();
        private static bool _initialized = false;

        /// <summary>
        /// Reinitialize the cache from the db.
        /// </summary>
        public static void Refresh()
        {

            _sync.EnterWriteLock();

            Create();

            try
            {
                _initialized = true;
                _cache = new Dictionary<string, ApplicationConfigSetting>();
                var context = DbContext();

                using (context)
                {
                    foreach (var configSetting in context.Db.Select<ApplicationConfigSetting>())
                    {
                        if (!_cache.ContainsKey(configSetting.Key))
                            _cache.Add(configSetting.Key, configSetting);
                    }
                }
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        /// <summary>
        /// Create the config if it doesn't exist.
        /// </summary>
        private static void Create()
        {
            var context = DbContext();

            using (context)
            {
                context.Db.CreateTable<ApplicationConfigSetting>(overwrite: false);

                if (context.Db.Count<ApplicationConfigSetting>() == 0)
                {
                    // misc settings go here
                }
            }
        }

        /// <summary>
        /// Static dispose. Mostly for unit tests.
        /// 
        /// (sometimes we may want to clean up the sqllite db)
        /// </summary>
        public static void Cleanup()
        {
            _sync.EnterWriteLock();
            try
            {
                var localpath = LocalDbPath;

                if (File.Exists(localpath))
                {
                    //try { File.Delete(localpath); }
                    //catch { }
                }
            }
            finally
            {
                _sync.ExitWriteLock();
            }
        }

        /// <summary>
        /// Read a value from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static ApplicationConfigSetting ReadValue(string key)
        {
            bool init = false;

            _sync.EnterReadLock();
            init = _initialized;
            _sync.ExitReadLock();

            if (!init)
                Refresh();

            // look for the value in the cache
            _sync.EnterReadLock();
            try
            {
                if (_cache.ContainsKey(key))
                    return _cache[key];
            }
            finally
            {
                _sync.ExitReadLock();
            }

            return null;
        }

        private static string StringOrNull(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(appSetting))
                return appSetting;

            var setting = ReadValue(key);
            if (null == setting) return null;

            return setting.Value;
        }

        /// <summary>
        /// Used to store config values.
        /// </summary>
        internal class ApplicationConfigSetting
        {
            [PrimaryKey, AutoIncrement, Alias("ApplicationConfigSettingId")]
            public long Id { get; set; }

            public string Key { get; set; }

            public string Value { get; set; }

            public T ValueAsJson<T>()
            {
                return JsonConvert.DeserializeObject<T>(this.Value);
            }

            public void SaveAsJson<T>(T value)
            {
                this.Value = JsonConvert.SerializeObject(value);
            }
        }

        #endregion
    }
}
