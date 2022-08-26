using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : ISession
    {
        #region QueryPage<T> 分页查询列表
        /// <summary>
        /// 分页查询列表
        /// </summary>
        public List<T> QueryPage<T>(string sql, string orderby, int pageSize, int currentPage) where T : new()
        {
            sql = _provider.CreatePageSql(sql, orderby, pageSize, currentPage);

            return QueryList<T>(sql);
        }
        #endregion

        #region QueryPageAsync<T> 分页查询列表
        /// <summary>
        /// 分页查询列表
        /// </summary>
        public async Task<List<T>> QueryPageAsync<T>(string sql, string orderby, int pageSize, int currentPage) where T : new()
        {
            sql = _provider.CreatePageSql(sql, orderby, pageSize, currentPage);

            return await QueryListAsync<T>(sql);
        }
        #endregion

        #region QueryPage<T> 分页查询列表(参数化查询)
        /// <summary>
        /// 分页查询列表
        /// </summary>
        public List<T> QueryPage<T>(string sql, string orderby, int pageSize, int currentPage, DbParameter[] cmdParms) where T : new()
        {
            sql = _provider.CreatePageSql(sql, orderby, pageSize, currentPage);

            return QueryList<T>(sql, cmdParms);
        }

        /// <summary>
        /// 分页查询列表
        /// </summary>
        public async Task<List<T>> QueryPageAsync<T>(string sql, string orderby, int pageSize, int currentPage, DbParameter[] cmdParms) where T : new()
        {
            sql = _provider.CreatePageSql(sql, orderby, pageSize, currentPage);

            return await QueryListAsync<T>(sql, cmdParms);
        }
        #endregion

    }
}
