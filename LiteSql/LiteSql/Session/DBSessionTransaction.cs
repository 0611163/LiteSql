using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : IDBSession
    {
        #region 开始事务
        /// <summary>
        /// 开始事务
        /// </summary>
        public DbTransactionExt BeginTransaction()
        {
            _conn = _connFactory.GetConnection(null);
            try
            {
                _tran = new DbTransactionExt(_conn.Conn.BeginTransaction(), _conn);
            }
            catch
            {
                _conn.Tran = null;
                _connFactory.Release(_conn);
                _tran = null;
                throw;
            }
            return _tran;
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
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_tran != null)
                {
                    _tran.Tran.Dispose();
                    _tran.Tran = null;
                    _tran = null;
                    _conn.Tran = null;
                    _connFactory.Release(_conn);
                }
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

            try
            {
                _tran.Tran.Rollback();
            }
            finally
            {
                if (_tran != null)
                {
                    _tran.Tran.Dispose();
                    _tran.Tran = null;
                    _tran = null;
                    _conn.Tran = null;
                    _connFactory.Release(_conn);
                }
            }
        }
        #endregion

    }
}
