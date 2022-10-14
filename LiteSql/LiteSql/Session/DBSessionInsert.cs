using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : IDBSession
    {
        #region Insert 添加
        /// <summary>
        /// 添加
        /// </summary>
        public void Insert(object obj)
        {
            StringBuilder strSql = new StringBuilder();
            int savedCount = 0;
            DbParameter[] parameters = null;

            PrepareInsertSql(obj, _autoIncrement, ref strSql, ref parameters, ref savedCount);

            Execute(strSql.ToString(), parameters);
        }

        /// <summary>
        /// 添加并返回ID
        /// </summary>
        public long InsertReturnId(object obj, string selectIdSql)
        {
            StringBuilder strSql = new StringBuilder();
            int savedCount = 0;
            DbParameter[] parameters = null;

            PrepareInsertSql(obj, _autoIncrement, ref strSql, ref parameters, ref savedCount);
            strSql.Append(";" + selectIdSql + ";");

            OnExecuting?.Invoke(strSql.ToString(), parameters);

            object id = ExecuteScalar(strSql.ToString(), parameters);
            return Convert.ToInt64(id);
        }
        #endregion

        #region InsertAsync 添加
        /// <summary>
        /// 添加
        /// </summary>
        public Task InsertAsync(object obj)
        {
            StringBuilder strSql = new StringBuilder();
            int savedCount = 0;
            DbParameter[] parameters = null;

            PrepareInsertSql(obj, _autoIncrement, ref strSql, ref parameters, ref savedCount);

            return ExecuteAsync(strSql.ToString(), parameters);
        }

        /// <summary>
        /// 添加并返回ID
        /// </summary>
        public async Task<long> InsertReturnIdAsync(object obj, string selectIdSql)
        {
            StringBuilder strSql = new StringBuilder();
            int savedCount = 0;
            DbParameter[] parameters = null;

            PrepareInsertSql(obj, _autoIncrement, ref strSql, ref parameters, ref savedCount);
            strSql.Append(";" + selectIdSql + ";");

            OnExecuting?.Invoke(strSql.ToString(), parameters);

            object id = await ExecuteScalarAsync(strSql.ToString(), parameters);
            return Convert.ToInt64(id);
        }
        #endregion

        #region Insert<T> 批量添加
        /// <summary>
        /// 批量添加
        /// </summary>
        public void Insert<T>(List<T> list)
        {
            Insert<T>(list, 500);
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        public void Insert<T>(List<T> list, int pageSize)
        {
            for (int i = 0; i < list.Count; i += pageSize)
            {
                StringBuilder strSql = new StringBuilder();
                int savedCount = 0;
                DbParameter[] parameters = null;

                PrepareInsertSql<T>(list.Skip(i).Take(pageSize).ToList(), _autoIncrement, ref strSql, ref parameters, ref savedCount);

                Execute(strSql.ToString(), parameters);
            }
        }
        #endregion

        #region InsertAsync<T> 批量添加
        /// <summary>
        /// 批量添加
        /// </summary>
        public Task InsertAsync<T>(List<T> list)
        {
            return InsertAsync<T>(list, 500);
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        public async Task InsertAsync<T>(List<T> list, int pageSize)
        {
            for (int i = 0; i < list.Count; i += pageSize)
            {
                StringBuilder strSql = new StringBuilder();
                int savedCount = 0;
                DbParameter[] parameters = null;

                PrepareInsertSql<T>(list.Skip(i).Take(pageSize).ToList(), _autoIncrement, ref strSql, ref parameters, ref savedCount);

                await ExecuteAsync(strSql.ToString(), parameters);
            }
        }
        #endregion

        #region PrepareInsertSql 准备Insert的SQL
        /// <summary>
        /// 准备Insert的SQL
        /// </summary>
        private void PrepareInsertSql(object obj, bool autoIncrement, ref StringBuilder strSql, ref DbParameter[] parameters, ref int savedCount)
        {
            Type type = obj.GetType();
            strSql.Append(string.Format("insert into {0}(", GetTableName(_provider, type)));
            PropertyInfoEx[] propertyInfoList = GetEntityProperties(type);
            List<Tuple<string, Type>> propertyNameList = new List<Tuple<string, Type>>();
            foreach (PropertyInfoEx propertyInfoEx in propertyInfoList)
            {
                PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                if (IsAutoIncrementPk(type, propertyInfoEx, autoIncrement)) continue;

                if (propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false).Length > 0)
                {
                    propertyNameList.Add(new Tuple<string, Type>(propertyInfoEx.FieldName, propertyInfoEx.PropertyInfo.PropertyType));
                    savedCount++;
                }
            }

            strSql.Append(string.Format("{0})", string.Join(",", propertyNameList.ConvertAll(a => string.Format("{0}{1}{2}", _provider.OpenQuote, a.Item1, _provider.CloseQuote)).ToArray())));
            strSql.Append(string.Format(" values ({0})", string.Join(",", propertyNameList.ConvertAll<string>(a => _provider.GetParameterName(a.Item1, a.Item2)).ToArray())));
            parameters = new DbParameter[savedCount];
            int k = 0;
            for (int i = 0; i < propertyInfoList.Length && savedCount > 0; i++)
            {
                PropertyInfoEx propertyInfoEx = propertyInfoList[i];
                PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                if (IsAutoIncrementPk(type, propertyInfoEx, autoIncrement)) continue;

                if (propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false).Length > 0)
                {
                    object val = propertyInfo.GetValue(obj, null);
                    Type parameterType = val == null ? typeof(object) : val.GetType();
                    DbParameter param = _provider.GetDbParameter(_provider.GetParameterName(propertyInfoEx.FieldName, parameterType), val == null ? DBNull.Value : val);
                    parameters[k++] = param;
                }
            }
        }
        #endregion

        #region PrepareInsertSql 准备批量Insert的SQL
        /// <summary>
        /// 准备批量Insert的SQL
        /// </summary>
        private void PrepareInsertSql<T>(List<T> list, bool autoIncrement, ref StringBuilder strSql, ref DbParameter[] parameters, ref int savedCount)
        {
            Type type = typeof(T);
            strSql.Append(string.Format("insert into {0}(", GetTableName(_provider, type)));
            PropertyInfoEx[] propertyInfoList = GetEntityProperties(type);
            List<Tuple<string, Type>> propertyNameList = new List<Tuple<string, Type>>();
            foreach (PropertyInfoEx propertyInfoEx in propertyInfoList)
            {
                PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                if (IsAutoIncrementPk(type, propertyInfoEx, autoIncrement)) continue;

                if (propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false).Length > 0)
                {
                    propertyNameList.Add(new Tuple<string, Type>(propertyInfoEx.FieldName, propertyInfoEx.PropertyInfo.PropertyType));
                    savedCount++;
                }
            }

            strSql.Append(string.Format("{0}) values ", string.Join(",", propertyNameList.ConvertAll<string>(a => _provider.OpenQuote + a.Item1 + _provider.CloseQuote).ToArray())));
            for (int i = 0; i < list.Count; i++)
            {
                strSql.Append(string.Format(" ({0})", string.Join(",", propertyNameList.ConvertAll<string>(a => _provider.GetParameterName(a.Item1 + i, a.Item2)).ToArray())));
                if (i != list.Count - 1)
                {
                    strSql.Append(", ");
                }
            }

            parameters = new DbParameter[savedCount * list.Count];
            int k = 0;
            for (int n = 0; n < list.Count; n++)
            {
                T obj = list[n];
                for (int i = 0; i < propertyInfoList.Length && savedCount > 0; i++)
                {
                    PropertyInfoEx propertyInfoEx = propertyInfoList[i];
                    PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                    if (IsAutoIncrementPk(type, propertyInfoEx, autoIncrement)) continue;

                    if (propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false).Length > 0)
                    {
                        object val = propertyInfo.GetValue(obj, null);
                        Type parameterType = val == null ? typeof(object) : val.GetType();
                        DbParameter param = _provider.GetDbParameter(_provider.GetParameterName(propertyInfoEx.FieldName + n, parameterType), val == null ? DBNull.Value : val);
                        parameters[k++] = param;
                    }
                }
            }
        }
        #endregion

    }
}
