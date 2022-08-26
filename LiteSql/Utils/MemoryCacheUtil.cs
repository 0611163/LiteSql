using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// 缓存
    /// 缓存数据存储在内存中
    /// 适用于CS项目,BS项目慎用
    /// </summary>
    public static class MemoryCacheUtil
    {
        #region 变量
        /// <summary>
        /// 内存缓存
        /// </summary>
        private static ConcurrentDictionary<string, CacheData> _cacheDict = new ConcurrentDictionary<string, CacheData>();

        /// <summary>
        /// 对不同的键提供不同的锁，用于读缓存
        /// </summary>
        private static ConcurrentDictionary<string, string> _dictLocksForReadCache = new ConcurrentDictionary<string, string>();
        #endregion

        #region 获取并缓存数据
        /// <summary>
        /// 获取并缓存数据
        /// 高并发的情况建议使用此重载函数，防止重复写入内存缓存
        /// </summary>
        /// <param name="cacheKey">键</param>
        /// <param name="func">在此方法中初始化数据</param>
        /// <param name="expirationSeconds">缓存过期时间(秒)，0表示永不过期</param>
        /// <param name="refreshCache">立即刷新缓存</param>
        public static T TryGetValue<T>(string cacheKey, Func<T> func, int expirationSeconds = 0, bool refreshCache = false)
        {
            string pre = "MemoryCacheUtil.TryGetValue<T>";
            lock (_dictLocksForReadCache.GetOrAdd(pre + cacheKey, pre + cacheKey))
            {
                object cacheValue = MemoryCacheUtil.GetValue(cacheKey);
                if (cacheValue != null && !refreshCache)
                {
                    return (T)cacheValue;
                }
                else
                {
                    T value = func();
                    MemoryCacheUtil.SetValue(cacheKey, value, expirationSeconds);
                    return value;
                }
            }
        }
        #endregion

        #region SetValue 保存键值对
        /// <summary>
        /// 保存键值对
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        /// <param name="expirationSeconds">过期时间(秒)，0表示永不过期</param>
        internal static void SetValue(string key, object value, int expirationSeconds = 0)
        {
            try
            {
                CacheData data = new CacheData(key, value);
                data.updateTime = DateTime.Now;
                data.expirationSeconds = expirationSeconds;

                CacheData temp;
                _cacheDict.TryRemove(key, out temp);
                _cacheDict.TryAdd(key, data);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "MemoryCacheUtil写缓存错误");
            }
        }
        #endregion

        #region GetValue 获取键值对
        /// <summary>
        /// 获取键值对
        /// </summary>
        internal static object GetValue(string key)
        {
            try
            {
                CacheData data;
                if (_cacheDict.TryGetValue(key, out data))
                {
                    if (data.expirationSeconds > 0 && DateTime.Now.Subtract(data.updateTime).TotalSeconds > data.expirationSeconds)
                    {
                        CacheData temp;
                        _cacheDict.TryRemove(key, out temp);
                        return null;
                    }
                    return data.value;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "MemoryCacheUtil读缓存错误");
                return null;
            }
        }
        #endregion

        #region Delete 删除缓存
        /// <summary>
        /// 删除缓存
        /// </summary>
        internal static void Delete(string key)
        {
            CacheData temp;
            _cacheDict.TryRemove(key, out temp);
        }
        #endregion

        #region DeleteAll 删除全部缓存
        /// <summary>
        /// 删除全部缓存
        /// </summary>
        internal static void DeleteAll()
        {
            _cacheDict.Clear();
        }
        #endregion

    }
}
