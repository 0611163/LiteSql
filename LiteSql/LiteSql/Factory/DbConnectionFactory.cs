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
        /// 数据库连接集合, key:数据库Provider类型名称+下划线+数据库连接字符串, value:该数据库连接字符串对应的数据库连接集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, DbConnectionCollection> _connections = new ConcurrentDictionary<string, DbConnectionCollection>();

        /// <summary>
        /// 数据库连接超时释放时间
        /// </summary>
        private static readonly int _timeout = 5000;

        /// <summary>
        /// 连接池最小数量
        /// </summary>
        private static readonly int _minPoolSize = 10;

        /// <summary>
        /// 锁
        /// </summary>
        private static readonly object _lock = new object();

        #region DbConnectionFactory 静态构造函数
        static DbConnectionFactory()
        {
            //数据库连接释放定时器
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    try
                    {
                        foreach (string key in _connections.Keys)
                        {
                            DbConnectionCollection dbConnections = _connections[key];
                            while (dbConnections.Connections.TryDequeue(out DbConnectionExt connExt))
                            {
                                if (!connExt.IsUsing
                                   && DateTime.Now.Subtract(connExt.CreateTime).TotalSeconds > _timeout)
                                {
                                    connExt.Conn.Close();
                                }
                                else
                                {
                                    dbConnections.Connections.Enqueue(connExt);
                                }
                            }

                            if (dbConnections.Connections.Count < _minPoolSize)
                            {
                                for (int i = 0; i < _minPoolSize - dbConnections.Connections.Count; i++)
                                {
                                    //创建连接池
                                    DbConnection conn = dbConnections.Provider.CreateConnection(dbConnections.ConnnectionString);
                                    conn.Open();
                                    DbConnectionExt connExt = new DbConnectionExt(conn, dbConnections, false);
                                    dbConnections.Connections.Enqueue(connExt);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion

        #region GetConnection 从数据库连接池获取一个数据库连接
        /// <summary>
        /// 从数据库连接池获取一个数据库连接
        /// </summary>
        public static DbConnectionExt GetConnection(IProvider provider, string connnectionString, DbTransactionExt _tran)
        {
            if (_tran != null)
            {
                _tran.ConnEx.Tran = _tran;
                return _tran.ConnEx;
            }

            //获取或初始化DbConnectionCollection
            string key = provider.GetType().Name + "_" + connnectionString;
            DbConnectionCollection dbConnections = _connections.GetOrAdd(key, k => new DbConnectionCollection(provider, connnectionString));

            lock (_lock)
            {
                //从空闲数据库连接池中取数据库连接
                dbConnections.Connections.TryDequeue(out DbConnectionExt connectionExt);
                while (connectionExt != null && connectionExt.IsUsing)
                {
                    dbConnections.Connections.Enqueue(connectionExt);
                    dbConnections.Connections.TryDequeue(out connectionExt);
                }
                if (connectionExt != null)
                {
                    connectionExt.IsUsing = true;
                    return connectionExt;
                }
            }

            //连接池没有则创建
            DbConnection conn = provider.CreateConnection(connnectionString);
            conn.Open();
            DbConnectionExt connExt = new DbConnectionExt(conn, dbConnections);
            dbConnections.Connections.Enqueue(connExt);
            return connExt;
        }
        #endregion

        #region GetConnectionAsync 从数据库连接池获取一个数据库连接
        /// <summary>
        /// 从数据库连接池获取一个数据库连接
        /// </summary>
        public static async Task<DbConnectionExt> GetConnectionAsync(IProvider provider, string connnectionString, DbTransactionExt _tran)
        {
            if (_tran != null)
            {
                _tran.ConnEx.Tran = _tran;
                return _tran.ConnEx;
            }

            //获取或初始化DbConnectionCollection
            string key = provider.GetType().Name + "_" + connnectionString;
            DbConnectionCollection dbConnections = _connections.GetOrAdd(key, k => new DbConnectionCollection(provider, connnectionString));

            lock (_lock)
            {
                //从空闲数据库连接池中取数据库连接
                dbConnections.Connections.TryDequeue(out DbConnectionExt connectionExt);
                while (connectionExt != null && connectionExt.IsUsing)
                {
                    dbConnections.Connections.Enqueue(connectionExt);
                    dbConnections.Connections.TryDequeue(out connectionExt);
                }
                if (connectionExt != null)
                {
                    connectionExt.IsUsing = true;
                    return connectionExt;
                }
            }

            //连接池没有则创建
            DbConnection conn = provider.CreateConnection(connnectionString);
            await conn.OpenAsync();
            DbConnectionExt connExt = new DbConnectionExt(conn, dbConnections);
            dbConnections.Connections.Enqueue(connExt);
            return connExt;
        }
        #endregion

    }
}
