using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// 记录耗时
    /// 封装Stopwatch
    /// </summary>
    public class LogTimeUtil
    {
        private Stopwatch _stopwatch;

        /// <summary>
        /// 记录耗时
        /// </summary>
        public LogTimeUtil()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        /// 记录耗时，并停止计时
        /// </summary>
        public string LogTime(string msg, bool restart = false)
        {
            msg = msg + "，耗时：" + _stopwatch.Elapsed.TotalSeconds.ToString("0.000") + " 秒";
            LogUtil.Log(msg);
            _stopwatch.Stop();
            if (restart) _stopwatch.Restart();
            return msg;
        }
    }
}
