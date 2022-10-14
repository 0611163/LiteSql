using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// LiteSql客户端接口
    /// ILiteSqlClient是线程安全的
    /// </summary>
    public interface ILiteSqlClient
    {
        /// <summary>
        /// 获取 IDBSession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        IDBSession GetSession(SplitTableMapping splitTableMapping = null);

        /// <summary>
        /// 获取 IDBSession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        Task<IDBSession> GetSessionAsync(SplitTableMapping splitTableMapping = null);

    }
}
