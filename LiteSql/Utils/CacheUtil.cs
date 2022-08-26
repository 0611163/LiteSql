using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// 缓存
    /// 内存缓存和文件缓存混合
    /// </summary>
    public static class CacheUtil
    {
        #region 变量
        /// <summary>
        /// 对不同的键提供不同的锁，用于读缓存
        /// </summary>
        private static ConcurrentDictionary<string, string> _dictLocksForReadCache = new ConcurrentDictionary<string, string>();
        #endregion

        #region 获取并缓存数据
        /// <summary>
        /// 获取并缓存数据
        /// 高并发的情况建议使用此重载函数，防止重复写入文件缓存
        /// </summary>
        /// <param name="cacheKey">键</param>
        /// <param name="func">在此方法中初始化数据</param>
        /// <param name="onlyMemoryCache">true数据只缓存在内存中 false数据缓存在内存和文件中</param>
        /// <param name="expirationSeconds">缓存过期时间(秒)，0表示永不过期</param>
        /// <param name="refreshCache">立即刷新缓存</param>
        public static T TryGetValue<T>(string cacheKey, Func<T> func, bool onlyMemoryCache = false, int expirationSeconds = 0, bool refreshCache = false)
        {
            string pre = "CacheUtil.TryGetValue<T>";
            lock (_dictLocksForReadCache.GetOrAdd(pre + cacheKey, pre + cacheKey))
            {
                object cacheValue = CacheUtil.GetValue(cacheKey, onlyMemoryCache);
                if (cacheValue != null && !refreshCache)
                {
                    return (T)cacheValue;
                }
                else
                {
                    T value = func();
                    CacheUtil.SetValue(cacheKey, value, onlyMemoryCache, expirationSeconds);
                    return value;
                }
            }
        }
        #endregion

        #region SetValue 保存键值对
        /// <summary>
        /// 保存键值对
        /// </summary>
        internal static void SetValue(string key, object value, bool onlyMemoryCache = false, int expirationSeconds = 0)
        {
            MemoryCacheUtil.SetValue(key, value, expirationSeconds);
            if (!onlyMemoryCache) //有的数据是不能缓存在文件中的，只能缓存在内存中
            {
                FileCacheUtil.SetValue(key, value, expirationSeconds);
            }
        }
        #endregion

        #region GetValue 获取键值对
        /// <summary>
        /// 获取键值对
        /// </summary>
        internal static object GetValue(string key, bool onlyMemoryCache = false)
        {
            lock (_dictLocksForReadCache.GetOrAdd(key, key))
            {
                object memoryCacheValue = MemoryCacheUtil.GetValue(key);
                if (memoryCacheValue != null)
                {
                    return memoryCacheValue;
                }
                else
                {
                    if (!onlyMemoryCache)
                    {
                        CacheData cacheData = FileCacheUtil.GetCacheData(key);
                        if (cacheData != null)
                        {
                            int expirationSeconds = (int)cacheData.updateTime.AddSeconds(cacheData.expirationSeconds).Subtract(DateTime.Now).TotalSeconds; //剩余过期时间
                            if (cacheData.expirationSeconds == 0) expirationSeconds = 0; //永不过期的情况
                            MemoryCacheUtil.SetValue(key, cacheData.value, expirationSeconds); //登录系统后(非首次登录)，MemoryCache为空，FileCache不为空
                            return cacheData.value;
                        }
                    }
                }
                return null;
            }
        }
        #endregion

        #region Delete 删除缓存
        /// <summary>
        /// 删除缓存
        /// </summary>
        public static void Delete(string key)
        {
            MemoryCacheUtil.Delete(key);
            FileCacheUtil.Delete(key);
        }
        #endregion

        #region DeleteAll 删除全部缓存
        /// <summary>
        /// 删除全部缓存
        /// </summary>
        public static void DeleteAll()
        {
            MemoryCacheUtil.DeleteAll();
            FileCacheUtil.DeleteAll();
        }
        #endregion

    }
}
