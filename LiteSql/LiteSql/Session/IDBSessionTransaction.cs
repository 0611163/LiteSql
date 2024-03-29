﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial interface IDbSession
    {
        #region 开始事务
        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTransaction(IsolationLevel isolationLevel);
        #endregion

        #region 提交事务
        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTransaction();
        #endregion

        #region 回滚事务(出错时调用该方法回滚)
        /// <summary>
        /// 回滚事务(出错时调用该方法回滚)
        /// </summary>
        void RollbackTransaction();
        #endregion

    }
}
