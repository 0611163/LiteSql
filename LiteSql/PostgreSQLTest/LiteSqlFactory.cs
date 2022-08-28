using LiteSql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSQLTest
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _liteSqlClient = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.PostgreSQL, new PostgreSQLProvider());
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        public static ISession GetSession()
        {
            return _liteSqlClient.GetSession();
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        public static async Task<ISession> GetSessionAsync()
        {
            return await _liteSqlClient.GetSessionAsync();
        }
        #endregion

    }
}
