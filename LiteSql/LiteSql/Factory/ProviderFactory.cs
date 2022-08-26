using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// 数据库实现工厂
    /// </summary>
    public class ProviderFactory
    {
        #region 变量属性
        private static ConcurrentDictionary<DBType, IProvider> _providers = new ConcurrentDictionary<DBType, IProvider>();
        private static ConcurrentDictionary<Type, IProvider> _providersByType = new ConcurrentDictionary<Type, IProvider>();
        #endregion

        #region 创建数据库 Provider
        /// <summary>
        /// 创建数据库 Provider
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        internal static IProvider CreateProvider(DBType dbType)
        {
            IProvider provider;
            _providers.TryGetValue(dbType, out provider);
            return provider;
        }

        /// <summary>
        /// 创建数据库 Provider
        /// </summary>
        /// <param name="providerType">数据库提供者类型</param>
        internal static IProvider CreateProvider(Type providerType)
        {
            IProvider provider;
            _providersByType.TryGetValue(providerType, out provider);
            return provider;
        }
        #endregion

        #region 注册Provider
        /// <summary>
        /// 注册数据库Provider(注册的数据库Provider需要继承相应的数据库提供者基类和IDBProvider,并重写IDBProvider中的接口实现)
        /// </summary>
        public static void RegisterDBProvider(DBType dbType, IProvider provider)
        {
            _providers.TryAdd(dbType, provider);
        }
        #endregion

        #region 注册Provider
        /// <summary>
        /// 注册数据库Provider(注册的数据库Provider必须实现IProvider接口)
        /// </summary>
        public static void RegisterDBProvider(Type providerType, IProvider provider)
        {
            _providersByType.TryAdd(providerType, provider);
        }
        #endregion

    }
}
