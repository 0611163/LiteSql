using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// LiteSql客户端接口
    /// ILiteSqlClient是线程安全的
    /// 请定义成单例模式
    /// </summary>
    public interface ILiteSqlClient<TFlag> : ILiteSqlClient
    {
        #region 获取 IDbSession
        /// <summary>
        /// 获取 IDbSession
        /// </summary>
        new IDbSession<TFlag> GetSession(SplitTableMapping splitTableMapping = null);
        #endregion

        #region 获取 IDbSession (异步)
        /// <summary>
        /// 获取 IDbSession (异步)
        /// </summary>
        new Task<IDbSession<TFlag>> GetSessionAsync(SplitTableMapping splitTableMapping = null);
        #endregion
    }

    /// <summary>
    /// LiteSql客户端接口
    /// ILiteSqlClient是线程安全的
    /// 请定义成单例模式
    /// </summary>
    public interface ILiteSqlClient
    {
        #region 获取 IDbSession
        /// <summary>
        /// 获取 IDbSession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        IDbSession GetSession(SplitTableMapping splitTableMapping = null);
        #endregion

        #region 获取 IDbSession (异步)
        /// <summary>
        /// 获取 IDbSession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        Task<IDbSession> GetSessionAsync(SplitTableMapping splitTableMapping = null);
        #endregion

        #region 创建SqlString对象
        /// <summary>
        /// 创建SqlString对象
        /// </summary>
        ISqlString Sql(string sql = null, params object[] args);

        /// <summary>
        /// 创建SqlString对象
        /// </summary>
        ISqlString<T> Sql<T>(string sql = null, params object[] args) where T : new();
        #endregion

        #region 创建SqlQueryable对象
        /// <summary>
        /// 创建IQueryable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        ISqlQueryable<T> Queryable<T>() where T : new();
        #endregion

        #region 查询下一个ID
        /// <summary>
        /// 查询下一个ID
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        int QueryNextId<T>();
        #endregion

        #region ForList
        /// <summary>
        /// 创建 in 或 not in SQL
        /// </summary>
        SqlValue ForList(IList list);
        #endregion

        #region 获取数据库连接
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        DbConnection GetConnection();

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        Task<DbConnection> GetConnectionAsync();
        #endregion

        #region SQL打印
        /// <summary>
        /// SQL打印
        /// </summary>
        Action<string, DbParameter[]> OnExecuting { get; set; }
        #endregion

    }
}
