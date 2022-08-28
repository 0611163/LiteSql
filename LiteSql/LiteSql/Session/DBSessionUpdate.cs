﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : ISession
    {
        #region 变量
        /// <summary>
        /// 新旧数据集合 key:新数据 value:旧数据
        /// </summary>
        private ConcurrentDictionary<object, object> _oldObjs = new ConcurrentDictionary<object, object>();
        #endregion

        #region AttachOld 附加更新前的旧实体
        /// <summary>
        /// 附加更新前的旧实体，只更新数据发生变化的字段
        /// </summary>
        public void AttachOld<T>(T obj) where T : new()
        {
            if (_oldObjs.ContainsKey(obj))
            {
                object temp;
                _oldObjs.TryRemove(obj, out temp);
            }

            if (!_oldObjs.ContainsKey(obj))
            {
                object cloneObj = ModelHelper.Convert<T>(obj);
                _oldObjs.TryAdd(obj, cloneObj);
            }
        }

        /// <summary>
        /// 附加更新前的旧实体，只更新数据发生变化的字段
        /// </summary>
        public void AttachOld<T>(List<T> objList) where T : new()
        {
            foreach (T obj in objList)
            {
                AttachOld(obj);
            }
        }
        #endregion

        #region Update 修改
        /// <summary>
        /// 修改
        /// </summary>
        public void Update(object obj)
        {
            //object oldObj = Find(obj);
            object oldObj;
            _oldObjs.TryGetValue(obj, out oldObj);
            if (oldObj == null) oldObj = Find(obj);

            if (oldObj == null) throw new Exception("无法获取到旧数据");

            StringBuilder strSql = new StringBuilder();
            int savedCount = 0;
            DbParameter[] parameters = null;
            PrepareUpdateSql(obj, oldObj, ref strSql, ref parameters, ref savedCount);

            if (savedCount > 0)
            {
                Execute(strSql.ToString(), parameters);
            }
        }
        #endregion

        #region UpdateAsync 修改
        /// <summary>
        /// 修改
        /// </summary>
        public async Task UpdateAsync(object obj)
        {
            //object oldObj = await FindAsync(obj);
            object oldObj;
            _oldObjs.TryGetValue(obj, out oldObj);
            if (oldObj == null) oldObj = Find(obj);

            if (oldObj == null) throw new Exception("无法获取到旧数据");

            StringBuilder strSql = new StringBuilder();
            int savedCount = 0;
            DbParameter[] parameters = null;
            PrepareUpdateSql(obj, oldObj, ref strSql, ref parameters, ref savedCount);

            if (savedCount > 0)
            {
                await ExecuteAsync(strSql.ToString(), parameters);
            }
        }
        #endregion

        #region Update<T> 批量修改
        /// <summary>
        /// 批量修改
        /// </summary>
        public void Update<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i += 500)
            {
                List<T> subList = list.Skip(i).Take(500).ToList();

                List<T> newList = new List<T>();
                List<T> oldList = new List<T>();
                for (int k = 0; k < subList.Count; k++)
                {
                    T obj = subList[k];
                    //object oldObj = Find(obj);
                    object oldObj;
                    _oldObjs.TryGetValue(obj, out oldObj);
                    if (oldObj == null) oldObj = Find(obj);

                    if (oldObj == null) throw new Exception("无法获取到旧数据");

                    newList.Add(obj);
                    oldList.Add((T)oldObj);
                }

                StringBuilder strSql = new StringBuilder();
                int savedCount = 0;
                DbParameter[] parameters = null;

                PrepareUpdateSql<T>(newList, oldList, ref strSql, ref parameters, ref savedCount);

                if (savedCount > 0)
                {
                    Execute(strSql.ToString(), parameters);
                }
            }
        }
        #endregion

        #region UpdateAsync<T> 批量修改
        /// <summary>
        /// 批量修改
        /// </summary>
        public async Task UpdateAsync<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i += 500)
            {
                List<T> subList = list.Skip(i).Take(500).ToList();

                List<T> newList = new List<T>();
                List<T> oldList = new List<T>();
                for (int k = 0; k < subList.Count; k++)
                {
                    T obj = subList[k];
                    //object oldObj = Find(obj);
                    object oldObj;
                    _oldObjs.TryGetValue(obj, out oldObj);
                    if (oldObj == null) oldObj = Find(obj);

                    if (oldObj == null) throw new Exception("无法获取到旧数据");

                    newList.Add(obj);
                    oldList.Add((T)oldObj);
                }

                StringBuilder strSql = new StringBuilder();
                int savedCount = 0;
                DbParameter[] parameters = null;

                PrepareUpdateSql<T>(newList, oldList, ref strSql, ref parameters, ref savedCount);

                if (savedCount > 0)
                {
                    await ExecuteAsync(strSql.ToString(), parameters);
                }
            }
        }
        #endregion

        #region PrepareUpdateSql 准备Update的SQL
        /// <summary>
        /// 准备Update的SQL
        /// </summary>
        private void PrepareUpdateSql(object obj, object oldObj, ref StringBuilder strSql, ref DbParameter[] parameters, ref int savedCount)
        {
            Type type = obj.GetType();

            List<DbParameter> paramList = new List<DbParameter>();
            PropertyInfoEx[] propertyInfoList = GetEntityProperties(type);
            StringBuilder sbPros = new StringBuilder();
            foreach (PropertyInfoEx propertyInfoEx in propertyInfoList)
            {
                PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                if (propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false).Length > 0)
                {
                    object oldVal = propertyInfo.GetValue(oldObj, null);
                    object val = propertyInfo.GetValue(obj, null);
                    if (!object.Equals(oldVal, val))
                    {
                        sbPros.Append(string.Format(" {0}={1}{2},", string.Format("{0}{1}{2}", _provider.OpenQuote, propertyInfoEx.FieldName, _provider.CloseQuote), _parameterMark, propertyInfoEx.FieldName));
                        DbParameter param = _provider.GetDbParameter(_parameterMark + propertyInfoEx.FieldName, val == null ? DBNull.Value : val);
                        paramList.Add(param);
                        savedCount++;
                    }
                }
            }

            strSql.Append(string.Format("update {0} ", GetTableName(_provider, type)));
            strSql.Append(string.Format(" set "));
            parameters = paramList.ToArray();
            if (sbPros.Length > 0)
            {
                strSql.Append(sbPros.ToString(0, sbPros.Length - 1));
            }
            strSql.Append(string.Format(" where {0}", CreatePkCondition(_provider, obj.GetType(), obj)));
        }
        #endregion

        #region PrepareUpdateSql 准备批量Update的SQL
        /// <summary>
        /// 准备批量Update的SQL
        /// </summary>
        private void PrepareUpdateSql<T>(List<T> list, List<T> oldList, ref StringBuilder strSql, ref DbParameter[] parameters, ref int savedCount)
        {
            Type type = typeof(T);

            List<DbParameter> paramList = new List<DbParameter>();
            for (int n = 0; n < list.Count; n++)
            {
                T obj = list[n];
                T oldObj = oldList[n];

                int subSavedCount = 0;

                PropertyInfoEx[] propertyInfoList = GetEntityProperties(type);
                StringBuilder sbPros = new StringBuilder();
                foreach (PropertyInfoEx propertyInfoEx in propertyInfoList)
                {
                    PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                    if (propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false).Length > 0)
                    {
                        object oldVal = propertyInfo.GetValue(oldObj, null);
                        object val = propertyInfo.GetValue(obj, null);
                        if (!object.Equals(oldVal, val))
                        {
                            sbPros.Append(string.Format(" {0}={1}{2}{3},", string.Format("{0}{1}{2}", _provider.OpenQuote, propertyInfoEx.FieldName, _provider.CloseQuote), _parameterMark, propertyInfoEx.FieldName, n));
                            DbParameter param = _provider.GetDbParameter(_parameterMark + propertyInfoEx.FieldName + n, val);
                            paramList.Add(param);
                            subSavedCount++;
                        }
                    }
                }

                if (subSavedCount > 0)
                {
                    savedCount++;
                    strSql.Append(string.Format("update {0} ", GetTableName(_provider, type)));
                    strSql.Append(string.Format(" set "));
                    if (sbPros.Length > 0)
                    {
                        strSql.Append(sbPros.ToString(0, sbPros.Length - 1));
                    }
                    strSql.Append(string.Format(" where {0}; ", CreatePkCondition(_provider, obj.GetType(), obj)));
                }
            }

            parameters = paramList.ToArray();
        }
        #endregion

    }
}