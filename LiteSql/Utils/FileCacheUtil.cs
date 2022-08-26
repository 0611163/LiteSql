using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Reflection;
using System.Collections.Concurrent;

namespace Utils
{
    /// <summary>
    /// 缓存工具类
    /// 缓存数据存储在文件中
    /// </summary>
    internal static class FileCacheUtil
    {
        #region 变量
        /// <summary>
        /// 对不同的键提供不同的锁，用于读文件
        /// </summary>
        private static ConcurrentDictionary<string, string> _dictLocksForReadFile = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 对不同的键提供不同的锁，用于写文件
        /// </summary>
        private static ConcurrentDictionary<string, string> _dictLocksForWriteFile = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 对不同的键提供不同的锁，用于删除文件
        /// </summary>
        private static ConcurrentDictionary<string, string> _dictLocksForDeleteFile = new ConcurrentDictionary<string, string>();
        private static BinaryFormatter formatter = new BinaryFormatter();
        private static string _folderPath;
        #endregion

        #region 静态构造函数
        static FileCacheUtil()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _folderPath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path)) + "\\cache";
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
                LogTimeUtil log = new LogTimeUtil();
                CacheData data = new CacheData(key, value);
                data.updateTime = DateTime.Now;
                data.expirationSeconds = expirationSeconds;

                if (!Directory.Exists(_folderPath))
                {
                    Directory.CreateDirectory(_folderPath);
                }

                string keyMd5 = GetMD5(key);
                string path = _folderPath + "\\" + keyMd5 + ".txt";
                lock (_dictLocksForWriteFile.GetOrAdd(key, key))
                {
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        fs.SetLength(0);
                        formatter.Serialize(fs, data);
                        fs.Close();
                    }
                }
                log.LogTime("FileCacheUtil 写缓存 " + key);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "FileCacheUtil写缓存错误");
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
                LogTimeUtil log = new LogTimeUtil();
                string keyMd5 = GetMD5(key);
                string path = _folderPath + "\\" + keyMd5 + ".txt";
                lock (_dictLocksForReadFile.GetOrAdd(key, key))
                {
                    if (File.Exists(path))
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            CacheData data = (CacheData)formatter.Deserialize(fs);
                            fs.Close();
                            if (data.expirationSeconds > 0 && DateTime.Now.Subtract(data.updateTime).TotalSeconds > data.expirationSeconds)
                            {
                                File.Delete(path);
                                return null;
                            }
                            log.LogTime("FileCacheUtil 读缓存 " + key);
                            return data.value;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "FileCacheUtil读缓存错误");
                return null;
            }
        }
        #endregion

        #region GetCacheData 获取键值对
        /// <summary>
        /// 获取键值对
        /// </summary>
        internal static CacheData GetCacheData(string key)
        {
            try
            {
                LogTimeUtil log = new LogTimeUtil();
                string keyMd5 = GetMD5(key);
                string path = _folderPath + "\\" + keyMd5 + ".txt";
                lock (_dictLocksForReadFile.GetOrAdd(key, key))
                {
                    if (File.Exists(path))
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            CacheData data = (CacheData)formatter.Deserialize(fs);
                            fs.Close();
                            if (data.expirationSeconds > 0 && DateTime.Now.Subtract(data.updateTime).TotalSeconds > data.expirationSeconds)
                            {
                                File.Delete(path);
                                return null;
                            }
                            log.LogTime("FileCacheUtil 读缓存 " + key);
                            return data;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "FileCacheUtil读缓存错误");
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
            string keyMd5 = GetMD5(key);
            string path = _folderPath + "\\" + keyMd5 + ".txt";
            lock (_dictLocksForDeleteFile.GetOrAdd(key, key))
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        #endregion

        #region DeleteAll 删除全部缓存
        /// <summary>
        /// 删除全部缓存
        /// </summary>
        internal static void DeleteAll()
        {
            string[] files = Directory.GetFiles(_folderPath);
            lock (_dictLocksForDeleteFile.GetOrAdd("FileCacheUtil.DeleteAll", "FileCacheUtil.DeleteAll"))
            {
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
        #endregion

        #region 计算MD5值
        /// <summary>
        /// 计算MD5值
        /// </summary>
        private static string GetMD5(string value)
        {
            string base64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(value)).Replace("/", "-");
            if (base64.Length > 200)
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bArr = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(value));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bArr)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
            return base64;
        }
        #endregion

    }

    #region CacheData 缓存数据
    /// <summary>
    /// 缓存数据
    /// </summary>
    [Serializable]
    public class CacheData
    {
        /// <summary>
        /// 键
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object value { get; set; }
        /// <summary>
        /// 缓存更新时间
        /// </summary>
        public DateTime updateTime { get; set; }
        /// <summary>
        /// 过期时间(秒)，0表示永不过期
        /// </summary>
        public int expirationSeconds { get; set; }

        /// <summary>
        /// 缓存数据
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        public CacheData(string key, object value)
        {
            this.key = key;
            this.value = value;
        }
    }
    #endregion

}
