using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public class SqlValue
    {
        public string Sql { get; set; }

        public object Value { get; set; }

        public SqlValue(string sql, object value)
        {
            Sql = sql;
            Value = value;
        }
    }
}
