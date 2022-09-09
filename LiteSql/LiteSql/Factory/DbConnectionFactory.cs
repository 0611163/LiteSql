using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LiteSql
{
    /// <summary>
    /// 数据库连接工厂
    /// </summary>
    internal static class DbConnectionFactory
    {
        /// <summary>
        /// 数据库连接池集合, key:数据库Provider类型名称+下划线+数据库连接字符串, value:数据库连接池
        /// </summary>
        private static readonly ConcurrentDictionary<string, DbConnectionCollection> _connectionPools = new ConcurrentDictionary<string, DbConnectionCollection>();

        /// <summary>
        /// 数据库连接超时释放时间
        /// </summary>
        private static readonly int _timeout = 10;

        /// <summary>
        /// 连接池最小数量
        /// </summary>
        private static readonly int _minPoolSize = 10;

        /// <summary>
        /// 定时器
        /// </summary>
        private static readonly Timer _timer;

        #region 静态构造函数
        static DbConnectionFactory()
        {
            _timer = new Timer(obj =>
            {
                try
                {
                    foreach (DbConnectionCollection connectionPool in _connectionPools.Values)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            if (connectionPool.Connections.TryDequeue(out DbConnectionExt connExt))
                            {
                                //数据库连接过期重新创建
                                if (DateTime.Now.Subtract(connExt.UpdateTime).TotalSeconds > _timeout)
                                {
                                    connExt.Conn.Close();
                                    DbConnection conn = connectionPool.Provider.CreateConnection(connectionPool.ConnnectionString);
                                    connExt = new DbConnectionExt(conn, connectionPool.Provider, connectionPool.ConnnectionString);
                                }

                                connectionPool.Connections.Enqueue(connExt);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }, null, 1000, 1000);
        }
        #endregion

        #region 初始化数据库连接对象池
        /// <summary>
        /// 初始化数据库连接对象池
        /// </summary>
        public static void InitConnectionPool(IProvider provider, string connectionString, int maxPoolSize)
        {
            string key = GetConnectionPoolKey(provider, connectionString);
            DbConnectionCollection connectionPool = _connectionPools.GetOrAdd(key, k => new DbConnectionCollection(provider, connectionString));

            for (int i = 0; i < maxPoolSize; i++)
            {
                DbConnection conn = connectionPool.Provider.CreateConnection(connectionPool.ConnnectionString);
                DbConnectionExt connExt = new DbConnectionExt(conn, provider, connectionString);
                if (i < _minPoolSize)
                {
                    conn.Open();
                }
                connectionPool.Connections.Enqueue(connExt);
            }
        }
        #endregion

        #region 获取连接池
        /// <summary>
        /// 获取连接池
        /// </summary>
        internal static DbConnectionCollection GetConnectionPool(IProvider provider, string connectionString)
        {
            string key = GetConnectionPoolKey(provider, connectionString);
            _connectionPools.TryGetValue(key, out DbConnectionCollection connctionPool);
            return connctionPool;
        }
        #endregion

        #region GetConnection 从数据库连接池获取一个数据库连接
        /// <summary>
        /// 从数据库连接池获取一个数据库连接
        /// </summary>
        public static DbConnectionExt GetConnection(IProvider provider, string connectionString, DbTransactionExt _tran)
        {
            if (_tran != null)
            {
                return _tran.ConnEx;
            }

            //获取连接池
            DbConnectionCollection connectionPool = GetConnectionPool(provider, connectionString);

            SpinWait spinWait = new SpinWait();
            DbConnectionExt connExt;
            while (!connectionPool.Connections.TryDequeue(out connExt))
            {
                spinWait.SpinOnce();
            }
            if (connExt.Conn.State != ConnectionState.Open)
            {
                connExt.Conn.Open();
            }
            return connExt;
        }
        #endregion

        #region GetConnectionAsync 从数据库连接池获取一个数据库连接
        /// <summary>
        /// 从数据库连接池获取一个数据库连接
        /// </summary>
        public static Task<DbConnectionExt> GetConnectionAsync(IProvider provider, string connectionString, DbTransactionExt _tran)
        {
            if (_tran != null)
            {
                return Task.FromResult(_tran.ConnEx);
            }

            //获取连接池
            DbConnectionCollection connectionPool = GetConnectionPool(provider, connectionString);

            SpinWait spinWait = new SpinWait();
            DbConnectionExt connExt;
            while (!connectionPool.Connections.TryDequeue(out connExt))
            {
                spinWait.SpinOnce();
            }
            if (connExt.Conn.State != ConnectionState.Open)
            {
                connExt.Conn.Open();
            }
            return Task.FromResult(connExt);
        }
        #endregion

        #region 回收数据库连接
        /// <summary>
        /// 回收数据库连接
        /// </summary>
        internal static void Release(DbConnectionExt connExt)
        {
            //获取连接池
            DbConnectionCollection connectionPool = GetConnectionPool(connExt.Provider, connExt.ConnectionString);

            //数据库连接过期重新创建
            if (DateTime.Now.Subtract(connExt.UpdateTime).TotalSeconds > _timeout)
            {
                connExt.Conn.Close();
                DbConnection conn = connectionPool.Provider.CreateConnection(connectionPool.ConnnectionString);
                connExt = new DbConnectionExt(conn, connectionPool.Provider, connectionPool.ConnnectionString);
            }

            connectionPool.Connections.Enqueue(connExt);
        }
        #endregion

        #region 获取连接池的键
        /// <summary>
        /// 获取连接池的键
        /// </summary>
        internal static string GetConnectionPoolKey(IProvider provider, string connectionString)
        {
            return provider.GetType().Name + "_" + connectionString;
        }
        #endregion

    }
}
