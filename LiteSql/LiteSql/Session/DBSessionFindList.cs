using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : ISession
    {
        #region QueryList<T> 查询列表
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(string sql) where T : new()
        {
            List<T> list = new List<T>();
            IDataReader rd = null;

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                try
                {
                    rd = ExecuteReader(sql, _conn);

                    DataReaderToList(rd, ref list);
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

            return list;
        }
        #endregion

        #region QueryListAsync<T> 查询列表
        /// <summary>
        /// 查询列表
        /// </summary>
        public async Task<List<T>> QueryListAsync<T>(string sql) where T : new()
        {
            List<T> list = new List<T>();
            IDataReader rd = null;

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                try
                {
                    rd = await ExecuteReaderAsync(sql, _conn);

                    DataReaderToList(rd, ref list);
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

            return list;
        }
        #endregion


        #region QueryList<T> 查询列表(参数化查询)
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(string sql, DbParameter[] cmdParms) where T : new()
        {
            List<T> list = new List<T>();
            IDataReader rd = null;

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                try
                {
                    rd = ExecuteReader(sql, cmdParms, _conn);

                    DataReaderToList(rd, ref list);
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

            return list;
        }
        #endregion

        #region QueryListAsync<T> 查询列表(参数化查询)
        /// <summary>
        /// 查询列表
        /// </summary>
        public async Task<List<T>> QueryListAsync<T>(string sql, DbParameter[] cmdParms) where T : new()
        {
            List<T> list = new List<T>();
            IDataReader rd = null;

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                try
                {
                    rd = await ExecuteReaderAsync(sql, cmdParms, _conn);

                    DataReaderToList(rd, ref list);
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

            return list;
        }
        #endregion


        #region 查询列表(传SqlString)
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(ISqlString sql) where T : new()
        {
            return QueryList<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        public Task<List<T>> QueryListAsync<T>(ISqlString sql) where T : new()
        {
            return QueryListAsync<T>(sql.SQL, sql.Params);
        }
        #endregion


        #region DataReaderToList
        /// <summary>
        /// DataReaderToList
        /// </summary>
        private void DataReaderToList<T>(IDataReader rd, ref List<T> list) where T : new()
        {
            if (typeof(T) == typeof(int))
            {
                while (rd.Read())
                {
                    list.Add((T)rd[0]);
                }
            }
            else if (typeof(T) == typeof(string))
            {
                while (rd.Read())
                {
                    list.Add((T)rd[0]);
                }
            }
            else
            {
                PropertyInfoEx[] propertyInfoList = GetEntityProperties(typeof(T));

                int fcnt = rd.FieldCount;
                Dictionary<string, string> fields = new Dictionary<string, string>();
                for (int i = 0; i < fcnt; i++)
                {
                    string field = rd.GetName(i).ToUpper();
                    if (!fields.ContainsKey(field))
                    {
                        fields.Add(field, null);
                    }
                }

                while (rd.Read())
                {
                    IDataRecord record = rd;
                    T obj = new T();

                    foreach (PropertyInfoEx propertyInfoEx in propertyInfoList)
                    {
                        PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                        if (!fields.ContainsKey(propertyInfoEx.FieldNameUpper)) continue;

                        object val = record[propertyInfoEx.FieldName];

                        if (val == DBNull.Value) continue;

                        val = val == DBNull.Value ? null : ConvertValue(val, propertyInfo.PropertyType);

                        propertyInfo.SetValue(obj, val);
                    }

                    list.Add((T)obj);
                }
            }
        }
        #endregion

    }
}
