using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/* ----------------------------------------------------------------------
* 作    者：suxiang
* 创建日期：2016年11月23日
* 更新日期：2022年08月28日
* ---------------------------------------------------------------------- */

namespace LiteSql
{
    public partial class DBSession : ISession
    {
        #region 静态变量
        /// <summary>
        /// SQL过滤正则
        /// </summary>
        private static Dictionary<string, Regex> _sqlFilteRegexDict = new Dictionary<string, Regex>();
        #endregion

        #region 变量
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// 事务
        /// </summary>
        private DbTransaction _tran;

        /// <summary>
        /// 数据库连接
        /// </summary>
        private DbConnection _conn;

        /// <summary>
        /// 数据库实现
        /// </summary>
        private IProvider _provider;

        /// <summary>
        /// 数据库自增(全局设置)
        /// </summary>
        private bool _autoIncrement;

        /// <summary>
        /// 分表映射
        /// </summary>
        private SplitTableMapping _splitTableMapping;
        #endregion

        #region 静态构造函数
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static DBSession()
        {
            _sqlFilteRegexDict.Add("net localgroup ", new Regex("net[\\s]+localgroup[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("net user ", new Regex("net[\\s]+user[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("xp_cmdshell ", new Regex("xp_cmdshell[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("exec ", new Regex("exec[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("execute ", new Regex("execute[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("truncate ", new Regex("truncate[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("drop ", new Regex("drop[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("restore ", new Regex("restore[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("create ", new Regex("create[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("alter ", new Regex("alter[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("rename ", new Regex("rename[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("insert ", new Regex("insert[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("update ", new Regex("update[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("delete ", new Regex("delete[\\s]+", RegexOptions.IgnoreCase));
            _sqlFilteRegexDict.Add("select ", new Regex("select[\\s]+", RegexOptions.IgnoreCase));
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public DBSession(string connectionString, DBType dbType, SplitTableMapping splitTableMapping, bool autoIncrement = false)
        {
            _connectionString = connectionString;
            _provider = ProviderFactory.CreateProvider(dbType);
            _splitTableMapping = splitTableMapping;
            _autoIncrement = autoIncrement;

            _conn = _provider.CreateConnection(_connectionString);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DBSession(string connectionString, Type providerType, SplitTableMapping splitTableMapping, bool autoIncrement = false)
        {
            _connectionString = connectionString;
            _provider = ProviderFactory.CreateProvider(providerType);
            _splitTableMapping = splitTableMapping;
            _autoIncrement = autoIncrement;

            _conn = _provider.CreateConnection(_connectionString);
        }
        #endregion

        #region 资源释放
        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (_conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
            if (_tran != null)
            {
                _tran.Dispose();
            }
        }
        #endregion

        #region InitConn 初始化数据库连接
        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        public void InitConn()
        {
            _conn.Open();
        }
        #endregion

        #region InitConnAsync 初始化数据库连接
        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        public async Task InitConnAsync()
        {
            await _conn.OpenAsync();
        }
        #endregion

        #region 创建SqlString对象
        /// <summary>
        /// 创建SqlString对象
        /// </summary>
        public ISqlString CreateSql(string sql = null, params object[] args)
        {
            return new SqlString(_provider, this, sql, args);
        }
        #endregion

        #region 创建SqlString对象
        /// <summary>
        /// 创建SqlString对象
        /// </summary>
        public ISqlQueryable<T> CreateSql<T>(string sql = null, params object[] args) where T : new()
        {
            return new SqlString<T>(_provider, this, sql, args);
        }

        /// <summary>
        /// 创建IQueryable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="alias">别名，默认值t</param>
        public ISqlQueryable<T> Queryable<T>(string alias = null) where T : new()
        {
            SqlString<T> sqlString = new SqlString<T>(_provider, this, null);
            return sqlString.Queryable(alias);
        }
        #endregion

        #region 查询下一个ID
        /// <summary>
        /// 查询下一个ID
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        public int QueryNextId<T>()
        {
            Type type = typeof(T);

            string idName = GetIdName(type, out _);
            string sql = _provider.CreateGetMaxIdSql(GetTableName(_provider, type), _provider.OpenQuote + idName + _provider.CloseQuote);

            using (IDbCommand cmd = _provider.GetCommand(sql, _conn))
            {
                object obj = cmd.ExecuteScalar();
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    return 1;
                }
                else
                {
                    return int.Parse(obj.ToString()) + 1;
                }
            }
        }
        #endregion

    }
}
