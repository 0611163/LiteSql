using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// 参数化查询SQL字符串接口
    /// </summary>
    public interface ISqlString
    {
        #region 查询接口

        /// <summary>
        /// 查询实体
        /// </summary>
        T Query<T>() where T : new();

        /// <summary>
        /// 查询实体
        /// </summary>
        Task<T> QueryAsync<T>() where T : new();

        /// <summary>
        /// 查询列表
        /// </summary>
        List<T> QueryList<T>() where T : new();

        /// <summary>
        /// 查询列表
        /// </summary>
        Task<List<T>> QueryListAsync<T>() where T : new();

        /// <summary>
        /// 分页查询
        /// </summary>
        List<T> QueryPage<T>(string orderby, int pageSize, int currentPage) where T : new();

        /// <summary>
        /// 分页查询
        /// </summary>
        Task<List<T>> QueryPageAsync<T>(string orderby, int pageSize, int currentPage) where T : new();

        /// <summary>
        /// 条件删除
        /// </summary>
        int DeleteByCondition<T>();

        /// <summary>
        /// 条件删除
        /// </summary>
        Task<int> DeleteByConditionAsync<T>();

        /// <summary>
        /// 条件删除
        /// </summary>
        int DeleteByCondition(Type type);

        /// <summary>
        /// 条件删除
        /// </summary>
        Task<int> DeleteByConditionAsync(Type type);

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        int Execute();

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        Task<int> ExecuteAsync();

        /// <summary>
        /// 是否存在
        /// </summary>
        bool Exists();

        /// <summary>
        /// 是否存在
        /// </summary>
        Task<bool> ExistsAsync();

        /// <summary>
        /// 查询单个值
        /// </summary>
        object QuerySingle();

        /// <summary>
        /// 查询单个值
        /// </summary>
        T QuerySingle<T>();

        /// <summary>
        /// 查询单个值
        /// </summary>
        Task<object> QuerySingleAsync();

        /// <summary>
        /// 查询单个值
        /// </summary>
        Task<T> QuerySingleAsync<T>();

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        long QueryCount();

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        Task<long> QueryCountAsync();

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        long QueryCount(int pageSize, out long pageCount);

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        Task<CountResult> QueryCountAsync(int pageSize);

        #endregion

    }
}
