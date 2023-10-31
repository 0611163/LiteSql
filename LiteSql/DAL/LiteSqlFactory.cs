using LiteSql;
using System.Configuration;
using System.Threading.Tasks;

namespace DAL
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _db = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), new MySQLProvider());

        public static ILiteSqlClient Db => _db;
        #endregion

        #region 获取 IDBSession
        /// <summary>
        /// 获取 IDBSession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static IDbSession GetSession(SplitTableMapping splitTableMapping = null)
        {
            return _db.GetSession(splitTableMapping);
        }
        #endregion

        #region 获取 IDBSession (异步)
        /// <summary>
        /// 获取 IDBSession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static async Task<IDbSession> GetSessionAsync(SplitTableMapping splitTableMapping = null)
        {
            return await _db.GetSessionAsync(splitTableMapping);
        }
        #endregion

    }
}
