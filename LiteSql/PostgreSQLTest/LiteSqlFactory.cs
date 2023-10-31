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
        private static ILiteSql _liteSqlClient = new LiteSql.LiteSql(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.PostgreSQL, new PostgreSQLProvider());
        #endregion

        #region 获取 IDBSession
        /// <summary>
        /// 获取 IDBSession
        /// </summary>
        public static IDBSession GetSession()
        {
            return _liteSqlClient.GetSession();
        }
        #endregion

        #region 获取 IDBSession (异步)
        /// <summary>
        /// 获取 IDBSession (异步)
        /// </summary>
        public static async Task<IDBSession> GetSessionAsync()
        {
            return await _liteSqlClient.GetSessionAsync();
        }
        #endregion

    }
}
