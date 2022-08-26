using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Utils
{
    /// <summary>
    /// 服务帮助类
    /// </summary>
    public class ServiceHelper
    {
        public static ConcurrentDictionary<Type, object> _dict = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 获取对象
        /// </summary>
        public static T Get<T>() where T : new()
        {
            Type type = typeof(T);
            object obj = _dict.GetOrAdd(type, (key) => new T());

            return (T)obj;
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        public static T Get<T>(Func<T> func)
        {
            Type type = typeof(T);
            object obj = _dict.GetOrAdd(type, (key) => func());

            return (T)obj;
        }

    }
}