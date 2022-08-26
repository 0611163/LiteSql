using LiteSql;
using System.Configuration;
using System.Threading.Tasks;

namespace OracleTest
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _liteSqlClient = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.Oracle);
        #endregion

        #region 静态构造函数
        static LiteSqlFactory()
        {
            ProviderFactory.RegisterDBProvider(DBType.Oracle, new OracleProvider());
        }
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
