using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        [NotMapped]
        public string TestTemp { get; set; }
    }
}
