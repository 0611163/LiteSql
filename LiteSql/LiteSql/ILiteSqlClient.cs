using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// LiteSqlClient接口
    /// </summary>
    public interface ILiteSqlClient
    {
        /// <summary>
        /// 获取 ISession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        ISession GetSession(SplitTableMapping splitTableMapping = null);

        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        Task<ISession> GetSessionAsync(SplitTableMapping splitTableMapping = null);

    }
}
