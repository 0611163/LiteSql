using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : ISession
    {
        #region 开始事务
        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            _conn = DbConnectionFactory.GetConnection(_provider, _connectionString, null);
            _tran = new DbTransactionExt(_conn.Conn.BeginTransaction(), _conn);
        }
        #endregion

        #region 提交事务
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            if (_tran == null) return; //防止重复提交

            try
            {
                _tran.Tran.Commit();
            }
            catch
            {
                _tran.Tran.Rollback();
                throw;
            }
            finally
            {
                _tran.Tran.Dispose();
                _tran.Tran = null;
                _tran = null;
                _conn.Tran = null;
                _conn.IsUsing = false;
                _conn.Parent.Connections.Enqueue(_conn);
            }
        }
        #endregion

        #region 回滚事务(出错时调用该方法回滚)
        /// <summary>
        /// 回滚事务(出错时调用该方法回滚)
        /// </summary>
        public void RollbackTransaction()
        {
            if (_tran == null) return; //防止重复回滚

            _tran.Tran.Rollback();
        }
        #endregion

    }
}
