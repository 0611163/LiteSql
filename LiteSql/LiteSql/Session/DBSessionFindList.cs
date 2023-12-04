using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DbSession : IDbSession
    {
        #region QueryList<T> 查询列表
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(string sql)
        {
            SqlFilter(ref sql);
            OnExecuting?.Invoke(sql, null);

            var conn = GetConnection(_tran);

            try
            {
                IDataReader rd = ExecuteReader(sql, conn);

                return DataReaderToList<T>(rd);
            }
            finally
            {
                if (_tran == null)
                {
                    if (conn.State != ConnectionState.Closed) conn.Close();
                }
            }
        }
        #endregion

        #region QueryListAsync<T> 查询列表
        /// <summary>
        /// 查询列表
        /// </summary>
        public async Task<List<T>> QueryListAsync<T>(string sql)
        {
            SqlFilter(ref sql);
            OnExecuting?.Invoke(sql, null);

            var conn = GetConnection(_tran);

            try
            {
                IDataReader rd = await ExecuteReaderAsync(sql, conn);

                return DataReaderToList<T>(rd);
            }
            finally
            {
                if (_tran == null)
                {
                    if (conn.State != ConnectionState.Closed) conn.Close();
                }
            }
        }
        #endregion


        #region QueryList<T> 查询列表(参数化查询)
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(string sql, DbParameter[] cmdParms)
        {
            OnExecuting?.Invoke(sql, cmdParms);

            var conn = GetConnection(_tran);

            try
            {
                IDataReader rd = ExecuteReader(sql, cmdParms, conn);

                return DataReaderToList<T>(rd);
            }
            finally
            {
                if (_tran == null)
                {
                    if (conn.State != ConnectionState.Closed) conn.Close();
                }
            }
        }
        #endregion

        #region QueryListAsync<T> 查询列表(参数化查询)
        /// <summary>
        /// 查询列表
        /// </summary>
        public async Task<List<T>> QueryListAsync<T>(string sql, DbParameter[] cmdParms)
        {
            OnExecuting?.Invoke(sql, cmdParms);

            var conn = GetConnection(_tran);

            try
            {
                IDataReader rd = await ExecuteReaderAsync(sql, cmdParms, conn);

                return DataReaderToList<T>(rd);
            }
            finally
            {
                if (_tran == null)
                {
                    if (conn.State != ConnectionState.Closed) conn.Close();
                }
            }
        }
        #endregion

        #region 查询列表(传SqlString)
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(ISqlString sql)
        {
            return QueryList<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        public Task<List<T>> QueryListAsync<T>(ISqlString sql)
        {
            return QueryListAsync<T>(sql.SQL, sql.Params);
        }
        #endregion


        #region DataReaderToList
        /// <summary>
        /// DataReaderToList
        /// </summary>
        private List<T> DataReaderToList<T>(IDataReader rd)
        {
            List<T> list = new List<T>();

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
            else if (typeof(T) == typeof(object)) //支持QueryList<dynamic>即返回值List<dynamic>类型
            {
                int fcnt = rd.FieldCount;
                Dictionary<string, int> fields = new Dictionary<string, int>();
                for (int i = 0; i < fcnt; i++)
                {
                    string field = rd.GetName(i);
                    if (!fields.ContainsKey(field))
                    {
                        fields.Add(field, i);
                    }
                }

                while (rd.Read())
                {
                    dynamic obj = new ExpandoObject();

                    foreach (string field in fields.Keys)
                    {
                        ((IDictionary<string, object>)obj).Add(field, rd[fields[field]]);
                    }

                    list.Add(obj);
                }
            }
            else
            {
                PropertyInfoEx[] propertyInfoList = GetEntityProperties(typeof(T));

                int fcnt = rd.FieldCount;
                StringBuilder strFields = new StringBuilder();
                Dictionary<string, int> fields = new Dictionary<string, int>();
                Dictionary<string, Type> fieldTypes = new Dictionary<string, Type>();
                for (int i = 0; i < fcnt; i++)
                {
                    string field = rd.GetName(i).ToUpper();
                    if (!fields.ContainsKey(field))
                    {
                        fields.Add(field, i);
                        fieldTypes.Add(field, rd.GetFieldType(i));
                        strFields.Append("_" + field);
                    }
                }

                var func = ExpressionMapper.BindData<T>(propertyInfoList, fields, fieldTypes, strFields.ToString());

                while (rd.Read())
                {
                    T obj = func(rd);

                    list.Add(obj);
                }
            }

            return list;
        }
        #endregion

    }
}
