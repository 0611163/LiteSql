﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : IDBSession
    {
        #region QueryById<T> 根据Id查询实体
        /// <summary>
        /// 根据Id查询实体
        /// </summary>
        public T QueryById<T>(object id) where T : new()
        {
            Type type = typeof(T);

            DbParameter[] cmdParms = new DbParameter[1];
            Type idType;
            string idName = GetIdName(type, out idType);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            cmdParms[0] = _provider.GetDbParameter(_provider.GetParameterName(idName, idType), id);

            string sql = string.Format("select * from {0} where {1}={2}", GetTableName(_provider, type), idNameWithQuote, _provider.GetParameterName(idName, idType));

            object result = Find(type, sql, cmdParms);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        #region QueryByIdAsync<T> 根据Id查询实体
        /// <summary>
        /// 根据Id查询实体
        /// </summary>
        public async Task<T> QueryByIdAsync<T>(object id) where T : new()
        {
            Type type = typeof(T);

            DbParameter[] cmdParms = new DbParameter[1];
            Type idType;
            string idName = GetIdName(type, out idType);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            cmdParms[0] = _provider.GetDbParameter(_provider.GetParameterName(idName, idType), id);

            string sql = string.Format("select * from {0} where {0}={2}", GetTableName(_provider, type), idNameWithQuote, _provider.GetParameterName(idName, idType));

            object result = await FindAsync(type, sql, cmdParms);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion


        #region Query<T> 根据sql查询实体
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public T Query<T>(string sql) where T : new()
        {
            Type type = typeof(T);
            object result = Find(type, sql, null);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        #region QueryAsync<T> 根据sql查询实体
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public async Task<T> QueryAsync<T>(string sql) where T : new()
        {
            Type type = typeof(T);
            object result = await FindAsync(type, sql, null);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion


        #region Query<T> 根据sql查询实体(参数化查询)
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public T Query<T>(string sql, DbParameter[] args) where T : new()
        {
            Type type = typeof(T);
            object result = Find(type, sql, args);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        #region QueryAsync<T> 根据sql查询实体(参数化查询)
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public async Task<T> QueryAsync<T>(string sql, DbParameter[] args) where T : new()
        {
            Type type = typeof(T);
            object result = await FindAsync(type, sql, args);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion


        #region 根据sql查询实体(传SqlString)
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public T Query<T>(ISqlString sql) where T : new()
        {
            return Query<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public Task<T> QueryAsync<T>(ISqlString sql) where T : new()
        {
            return QueryAsync<T>(sql.SQL, sql.Params);
        }
        #endregion


        #region Find 根据实体查询实体
        /// <summary>
        /// 根据实体查询实体
        /// </summary>
        private object Find(object obj)
        {
            Type type = obj.GetType();

            string sql = string.Format("select * from {0} where {1}", GetTableName(_provider, type), CreatePkCondition(_provider, obj.GetType(), obj, 0, out DbParameter[] cmdParams));

            return Find(type, sql, cmdParams);
        }
        #endregion

        #region FindAsync 根据实体查询实体
        /// <summary>
        /// 根据实体查询实体
        /// </summary>
        private Task<object> FindAsync(object obj)
        {
            Type type = obj.GetType();

            string sql = string.Format("select * from {0} where {1}", GetTableName(_provider, type), CreatePkCondition(_provider, obj.GetType(), obj, 0, out DbParameter[] cmdParams));

            return FindAsync(type, sql, cmdParams);
        }
        #endregion

        #region Find 查询实体
        /// <summary>
        /// 查询实体
        /// </summary>
        private object Find(Type type, string sql, DbParameter[] cmdParams)
        {
            object result = Activator.CreateInstance(type);
            IDataReader rd = null;
            bool hasValue = false;

            using (_conn = _connFactory.GetConnection(_tran))
            {
                try
                {
                    if (cmdParams == null)
                    {
                        rd = ExecuteReader(sql, _conn);
                    }
                    else
                    {
                        rd = ExecuteReader(sql, cmdParams, _conn);
                    }

                    DataReaderToEntity(type, rd, ref result, ref hasValue);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (rd != null && !rd.IsClosed)
                    {
                        rd.Close();
                        rd.Dispose();
                    }
                }
            }

            if (hasValue)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region FindAsync 查询实体
        /// <summary>
        /// 查询实体
        /// </summary>
        private async Task<object> FindAsync(Type type, string sql, DbParameter[] cmdParams)
        {
            object result = Activator.CreateInstance(type);
            IDataReader rd = null;
            bool hasValue = false;

            using (_conn = _connFactory.GetConnection(_tran))
            {
                try
                {
                    if (cmdParams == null)
                    {
                        rd = await ExecuteReaderAsync(sql, _conn);
                    }
                    else
                    {
                        rd = await ExecuteReaderAsync(sql, cmdParams, _conn);
                    }

                    DataReaderToEntity(type, rd, ref result, ref hasValue);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (rd != null && !rd.IsClosed)
                    {
                        rd.Close();
                        rd.Dispose();
                    }
                }
            }

            if (hasValue)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region DataReaderToEntity
        /// <summary>
        /// DataReaderToEntity
        /// </summary>
        private void DataReaderToEntity(Type type, IDataReader rd, ref object result, ref bool hasValue)
        {
            PropertyInfoEx[] propertyInfoList = GetEntityProperties(type);

            int fieldCount = rd.FieldCount;
            StringBuilder strFields = new StringBuilder();
            Dictionary<string, int> fields = new Dictionary<string, int>();
            for (int i = 0; i < fieldCount; i++)
            {
                string field = rd.GetName(i).ToUpper();
                if (!fields.ContainsKey(field))
                {
                    fields.Add(field, i);
                    strFields.Append("_" + field);
                }
            }

            var func = ExpressionMapper.BindData(type, propertyInfoList, fields, strFields.ToString());

            while (rd.Read())
            {
                hasValue = true;

                result = func(rd);
            }
        }
        #endregion

    }
}
