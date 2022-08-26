using System.Data.Common;

namespace LiteSql
{
    /// <summary>
    /// 数据库实现接口
    /// </summary>
    public interface IDBProvider
    {
        /// <summary>
        /// 创建 DbConnection
        /// </summary>
        DbConnection CreateConnection(string connectionString);

        /// <summary>
        /// 生成 DbParameter
        /// </summary>
        DbParameter GetDbParameter(string name, object vallue);
    }
}
