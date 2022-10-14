using LiteSql;
using System.Configuration;
using System.Threading.Tasks;

namespace DAL
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _liteSqlClient = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.MySQL, new MySQLProvider());
        #endregion

        #region 获取 IDBSession
        /// <summary>
        /// 获取 IDBSession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static IDBSession GetSession(SplitTableMapping splitTableMapping = null)
        {
            return _liteSqlClient.GetSession(splitTableMapping);
        }
        #endregion

        #region 获取 IDBSession (异步)
        /// <summary>
        /// 获取 IDBSession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static async Task<IDBSession> GetSessionAsync(SplitTableMapping splitTableMapping = null)
        {
            return await _liteSqlClient.GetSessionAsync(splitTableMapping);
        }
        #endregion

    }
}
