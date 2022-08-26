using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// Task帮助类基类
    /// </summary>
    public class TaskHelper
    {
        #region 变量
        /// <summary>
        /// 处理器数
        /// </summary>
        private static int _processorCount = Environment.ProcessorCount;
        #endregion

        #region UI任务
        private static TaskSchedulerEx _UITask;
        /// <summary>
        /// UI任务(2-4个线程)
        /// </summary>
        public static TaskSchedulerEx UITask
        {
            get
            {
                if (_UITask == null) _UITask = new TaskSchedulerEx(2, 4);
                return _UITask;
            }
        }
        #endregion

        #region 菜单任务
        private static TaskSchedulerEx _MenuTask;
        /// <summary>
        /// 菜单任务(2-4个线程)
        /// </summary>
        public static TaskSchedulerEx MenuTask
        {
            get
            {
                if (_MenuTask == null) _MenuTask = new TaskSchedulerEx(2, 4);
                return _MenuTask;
            }
        }
        #endregion

        #region 计算任务
        private static TaskSchedulerEx _CalcTask;
        /// <summary>
        /// 计算任务(线程数：处理器数*2)
        /// </summary>
        public static TaskSchedulerEx CalcTask
        {
            get
            {
                if (_CalcTask == null) _CalcTask = new TaskSchedulerEx(_processorCount * 2, _processorCount * 2);
                return _CalcTask;
            }
        }
        #endregion

        #region 网络请求
        private static TaskSchedulerEx _RequestTask;
        /// <summary>
        /// 网络请求(8-32个线程)
        /// </summary>
        public static TaskSchedulerEx RequestTask
        {
            get
            {
                if (_RequestTask == null) _RequestTask = new TaskSchedulerEx(8, 32);
                return _RequestTask;
            }
        }
        #endregion

        #region 数据库任务
        private static TaskSchedulerEx _DBTask;
        /// <summary>
        /// 数据库任务(8-32个线程)
        /// </summary>
        public static TaskSchedulerEx DBTask
        {
            get
            {
                if (_DBTask == null) _DBTask = new TaskSchedulerEx(8, 32);
                return _DBTask;
            }
        }
        #endregion

        #region IO任务
        private static TaskSchedulerEx _IOTask;
        /// <summary>
        /// IO任务(8-32个线程)
        /// </summary>
        public static TaskSchedulerEx IOTask
        {
            get
            {
                if (_IOTask == null) _IOTask = new TaskSchedulerEx(8, 32);
                return _IOTask;
            }
        }
        #endregion

        #region 首页任务
        private static TaskSchedulerEx _MainPageTask;
        /// <summary>
        /// 首页任务(8-32个线程)
        /// </summary>
        public static TaskSchedulerEx MainPageTask
        {
            get
            {
                if (_MainPageTask == null) _MainPageTask = new TaskSchedulerEx(8, 32);
                return _MainPageTask;
            }
        }
        #endregion

        #region 图片加载任务
        private static TaskSchedulerEx _LoadImageTask;
        /// <summary>
        /// 图片加载任务(8-32个线程)
        /// </summary>
        public static TaskSchedulerEx LoadImageTask
        {
            get
            {
                if (_LoadImageTask == null) _LoadImageTask = new TaskSchedulerEx(8, 32);
                return _LoadImageTask;
            }
        }
        #endregion

        #region 浏览器任务
        private static TaskSchedulerEx _BrowserTask;
        /// <summary>
        /// 浏览器任务(2-4个线程)
        /// </summary>
        public static TaskSchedulerEx BrowserTask
        {
            get
            {
                if (_BrowserTask == null) _BrowserTask = new TaskSchedulerEx(2, 4);
                return _BrowserTask;
            }
        }
        #endregion

    }
}
