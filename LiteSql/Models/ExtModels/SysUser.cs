using System;
using System.Collections.Generic;
using System.Linq;
using LiteSql;

namespace Models
{
    /// <summary>
    /// 用户表
    /// </summary>
    [AutoIncrement]
    public partial class SysUser
    {
        /// <summary>
        /// 测试用的字段
        /// </summary>
        public string TestTemp { get; set; }
    }
}
