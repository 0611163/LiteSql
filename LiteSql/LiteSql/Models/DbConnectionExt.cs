using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// 数据库连接扩展
    /// </summary>
    internal class DbConnectionExt : IDisposable
    {
        /// <summary>
        /// 数据库事务
        /// </summary>
        public DbTransactionExt Tran { get; set; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        public DbConnection Conn { get; set; }

        /// <summary>
        /// 连接创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否使用中
        /// </summary>
        public bool IsUsing { get; set; }

        /// <summary>
        /// 数据库连接扩展
        /// </summary>
        public DbConnectionExt(DbConnection conn, bool isUsing = true)
        {
            Conn = conn;
            CreateTime = DateTime.Now;
            IsUsing = isUsing;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (Tran == null)
            {
                IsUsing = false;
            }
        }
    }

    #region DbConnectionCollection 数据库连接集合
    /// <summary>
    /// 数据库连接集合
    /// </summary>
    internal class DbConnectionCollection
    {
        /// <summary>
        /// key:数据库Provider类型名称+下划线+数据库连接字符串
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 数据库Provider
        /// </summary>
        public IProvider Provider { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnnectionString { get; set; }

        /// <summary>
        /// 数据库连接集合
        /// </summary>
        public ConcurrentDictionary<DbConnectionExt, object> Connections { get; set; }

        /// <summary>
        /// 数据库连接集合 构造函数
        /// </summary>
        public DbConnectionCollection(IProvider provider, string connnectionString)
        {
            Connections = new ConcurrentDictionary<DbConnectionExt, object>();
            Provider = provider;
            ConnnectionString = connnectionString;
            Key = provider.GetType().Name + "_" + connnectionString;
        }
    }
    #endregion

}
