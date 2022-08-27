﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : ISession
    {
        #region DeleteById<T> 根据Id删除
        /// <summary>
        /// 根据Id删除
        /// </summary>
        public int DeleteById<T>(int id)
        {
            return DeleteById<T>(id.ToString());
        }
        #endregion

        #region DeleteById<T> 根据Id删除
        /// <summary>
        /// 根据Id删除
        /// </summary>
        public int DeleteById<T>(long id)
        {
            return DeleteById<T>(id.ToString());
        }
        #endregion

        #region DeleteById<T> 根据Id删除
        /// <summary>
        /// 根据Id删除
        /// </summary>
        public int DeleteById<T>(string id)
        {
            Type type = typeof(T);
            StringBuilder sbSql = new StringBuilder();
            DbParameter[] cmdParms = new DbParameter[1];
            string idName = GetIdName(type);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            cmdParms[0] = _provider.GetDbParameter(_parameterMark + idName, id);
            sbSql.Append(string.Format("delete from {0} where {3}={1}{2}", GetTableName(_provider, type), _parameterMark, idName, idNameWithQuote));

            return Execute(sbSql.ToString(), cmdParms);
        }
        #endregion


        #region DeleteByIdAsync<T> 根据Id删除
        /// <summary>
        /// 根据Id删除
        /// </summary>
        public Task<int> DeleteByIdAsync<T>(long id)
        {
            return DeleteByIdAsync<T>(id.ToString());
        }
        #endregion

        #region DeleteByIdAsync<T> 根据Id删除
        /// <summary>
        /// 根据Id删除
        /// </summary>
        public Task<int> DeleteByIdAsync<T>(int id)
        {
            return DeleteByIdAsync<T>(id.ToString());
        }
        #endregion

        #region DeleteByIdAsync<T> 根据Id删除
        /// <summary>
        /// 根据Id删除
        /// </summary>
        public Task<int> DeleteByIdAsync<T>(string id)
        {
            Type type = typeof(T);
            StringBuilder sbSql = new StringBuilder();
            DbParameter[] cmdParms = new DbParameter[1];
            string idName = GetIdName(type);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            cmdParms[0] = _provider.GetDbParameter(_parameterMark + idName, id);
            sbSql.Append(string.Format("delete from {0} where {3}={1}{2}", GetTableName(_provider, type), _parameterMark, idName, idNameWithQuote));

            return ExecuteAsync(sbSql.ToString(), cmdParms);
        }
        #endregion


        #region BatchDeleteByIds<T> 根据Id集合删除
        /// <summary>
        /// 根据Id集合删除
        /// </summary>
        public int BatchDeleteByIds<T>(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return 0;

            Type type = typeof(T);
            StringBuilder sbSql = new StringBuilder();
            string[] idArr = ids.Split(',');
            DbParameter[] cmdParms = new DbParameter[idArr.Length];
            string idName = GetIdName(type);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            sbSql.AppendFormat("delete from {0} where {1} in (", GetTableName(_provider, type), idNameWithQuote);
            for (int i = 0; i < idArr.Length; i++)
            {
                cmdParms[i] = _provider.GetDbParameter(_parameterMark + idName + i, idArr[i]);
                sbSql.AppendFormat("{0}{1}{2},", _parameterMark, idName, i);
            }
            sbSql.Remove(sbSql.Length - 1, 1);
            sbSql.Append(")");

            return Execute(sbSql.ToString(), cmdParms);
        }
        #endregion

        #region BatchDeleteByIdsAsync<T> 根据Id集合删除
        /// <summary>
        /// 根据Id集合删除
        /// </summary>
        public Task<int> BatchDeleteByIdsAsync<T>(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) throw new Exception("ids 不能为空");

            Type type = typeof(T);
            StringBuilder sbSql = new StringBuilder();
            string[] idArr = ids.Split(',');
            DbParameter[] cmdParms = new DbParameter[idArr.Length];
            string idName = GetIdName(type);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            sbSql.AppendFormat("delete from {0} where {1} in (", GetTableName(_provider, type), idNameWithQuote);
            for (int i = 0; i < idArr.Length; i++)
            {
                cmdParms[i] = _provider.GetDbParameter(_parameterMark + idName + i, idArr[i]);
                sbSql.AppendFormat("{0}{1}{2},", _parameterMark, idName, i);
            }
            sbSql.Remove(sbSql.Length - 1, 1);
            sbSql.Append(")");

            return ExecuteAsync(sbSql.ToString(), cmdParms);
        }
        #endregion


        #region DeleteByCondition<T> 根据条件删除
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public int DeleteByCondition<T>(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return 0;

            Type type = typeof(T);
            return DeleteByCondition(type, condition);
        }
        #endregion

        #region DeleteByConditionAsync<T> 根据条件删除
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync<T>(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) throw new Exception("condition 不能为空"); ;

            Type type = typeof(T);
            return DeleteByConditionAsync(type, condition);
        }
        #endregion


        #region DeleteByCondition 根据条件删除
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public int DeleteByCondition(Type type, string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return 0;

            StringBuilder sbSql = new StringBuilder();
            SqlFilter(ref condition);
            sbSql.Append(string.Format("delete from {0} where {1}", GetTableName(_provider, type), condition));

            return Execute(sbSql.ToString());
        }
        #endregion

        #region DeleteByConditionAsync 根据条件删除
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync(Type type, string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) throw new Exception("condition 不能为空");

            StringBuilder sbSql = new StringBuilder();
            SqlFilter(ref condition);
            sbSql.Append(string.Format("delete from {0} where {1}", GetTableName(_provider, type), condition));

            return ExecuteAsync(sbSql.ToString());
        }
        #endregion


        #region DeleteByCondition<T> 根据条件删除(参数化查询)
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public int DeleteByCondition<T>(string condition, DbParameter[] cmdParms)
        {
            if (string.IsNullOrWhiteSpace(condition)) return 0;

            Type type = typeof(T);
            return DeleteByCondition(type, condition, cmdParms);
        }
        #endregion

        #region DeleteByConditionAsync<T> 根据条件删除(参数化查询)
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync<T>(string condition, DbParameter[] cmdParms)
        {
            if (string.IsNullOrWhiteSpace(condition)) throw new Exception("condition 不能为空"); ;

            Type type = typeof(T);
            return DeleteByConditionAsync(type, condition, cmdParms);
        }
        #endregion


        #region DeleteByCondition 根据条件删除(参数化查询)
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public int DeleteByCondition(Type type, string condition, DbParameter[] cmdParms)
        {
            if (string.IsNullOrWhiteSpace(condition)) return 0;

            StringBuilder sbSql = new StringBuilder();
            SqlFilter(ref condition);
            sbSql.Append(string.Format("delete from {0} where {1}", GetTableName(_provider, type), condition));

            return Execute(sbSql.ToString(), cmdParms);
        }
        #endregion

        #region DeleteByConditionAsync 根据条件删除(参数化查询)
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync(Type type, string condition, DbParameter[] cmdParms)
        {
            if (string.IsNullOrWhiteSpace(condition)) throw new Exception("condition 不能为空");

            StringBuilder sbSql = new StringBuilder();
            SqlFilter(ref condition);
            sbSql.Append(string.Format("delete from {0} where {1}", GetTableName(_provider, type), condition));

            return ExecuteAsync(sbSql.ToString(), cmdParms);
        }
        #endregion


        #region 传SqlString
        /// <summary>
        /// 根据条件删除
        /// </summary>
        public int DeleteByCondition<T>(SqlString sql)
        {
            return DeleteByCondition<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 根据条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync<T>(SqlString sql)
        {
            return DeleteByConditionAsync<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 根据条件删除
        /// </summary>
        public int DeleteByCondition(Type type, SqlString sql)
        {
            return DeleteByCondition(type, sql.SQL, sql.Params);
        }

        /// <summary>
        /// 根据条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync(Type type, SqlString sql)
        {
            return DeleteByConditionAsync(type, sql.SQL, sql.Params);
        }
        #endregion

    }
}
