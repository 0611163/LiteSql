using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LiteSql
{
    /// <summary>
    /// 有继承关系的实体类转换
    /// </summary>
    public static class ModelHelper
    {
        #region Model转换
        /// <summary>
        /// Model转换
        /// </summary>
        public static T Convert<T>(this object obj) where T : new()
        {
            if (obj == null) return default(T);
            Type sourceType = obj.GetType();
            Type targetType = typeof(T);
            T t = new T();

            PropertyInfoEx[] sourcePropertyInfoArr = DBSession.GetEntityProperties(sourceType);
            PropertyInfoEx[] targetPropertyInfoArr = DBSession.GetEntityProperties(targetType);
            Dictionary<string, PropertyInfo> dictTargetPropertyInfo = targetPropertyInfoArr.ToLookup(a => a.PropertyInfo.Name).ToDictionary(a => a.Key, b => b.First().PropertyInfo);

            if (sourcePropertyInfoArr != null)
            {
                foreach (PropertyInfoEx sourcePropertyInfoEx in sourcePropertyInfoArr)
                {
                    PropertyInfo sourcePropertyInfo = sourcePropertyInfoEx.PropertyInfo;
                    PropertyInfo targetPropertyInfo = null;
                    if (dictTargetPropertyInfo.TryGetValue(sourcePropertyInfo.Name, out targetPropertyInfo))
                    {
                        targetPropertyInfo.SetValue(t, sourcePropertyInfo.GetValue(obj, null), null);
                    }
                }
            }
            return t;
        }
        #endregion

    }
}
