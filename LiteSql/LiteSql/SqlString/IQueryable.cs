using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// 查询接口
    /// </summary>
    public interface ISqlQueryable<T> : ISqlString where T : new()
    {
        /// <summary>
        /// 转成ISqlString接口
        /// </summary>
        ISqlString AsISqlString();

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="expression">Lambda 表达式</param>
        ISqlQueryable<T> WhereIf(bool condition, Expression<Func<T, object>> expression);

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="expression">Lambda 表达式</param>
        ISqlQueryable<T> WhereIf<U>(bool condition, Expression<Func<U, object>> expression);

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        ISqlQueryable<T> Where(Expression<Func<T, object>> expression);

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        ISqlQueryable<T> Where<U>(Expression<Func<U, object>> expression);

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        ISqlQueryable<T> Where<U>(Expression<Func<T, U, object>> expression);

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        ISqlQueryable<T> Where<U, D>(Expression<Func<T, U, D, object>> expression);

        /// <summary>
        /// 追加 order by SQL
        /// </summary>
        ISqlQueryable<T> OrderBy(Expression<Func<T, object>> expression);

        /// <summary>
        /// 追加 order by SQL
        /// </summary>
        ISqlQueryable<T> OrderByDescending(Expression<Func<T, object>> expression);

        /// <summary>
        /// 追加 left join SQL
        /// </summary>
        ISqlQueryable<T> LeftJoin<U>(Expression<Func<T, U, object>> expression);

        /// <summary>
        /// 追加 select SQL
        /// </summary>
        ISqlQueryable<T> Select<U>(Expression<Func<U, object>> expression, Expression<Func<T, object>> expression2);

        /// <summary>
        /// 执行查询
        /// </summary>
        List<T> ToList();

        /// <summary>
        /// 执行查询
        /// </summary>
        Task<List<T>> ToListAsync();

        /// <summary>
        /// 执行查询
        /// </summary>
        List<T> ToPageList(int page, int pageSize);

        /// <summary>
        /// 执行查询
        /// </summary>
        Task<List<T>> ToPageListAsync(int page, int pageSize);

        /// <summary>
        /// 返回数量
        /// </summary>
        long Count();

        /// <summary>
        /// 返回数量
        /// </summary>
        Task<long> CountAsync();

        /// <summary>
        /// 返回数量
        /// </summary>
        T First();

        /// <summary>
        /// 返回数量
        /// </summary>
        Task<T> FirstAsync();

        /// <summary>
        /// 是否存在
        /// </summary>
        new bool Exists();

        /// <summary>
        /// 返回数量
        /// </summary>
        new Task<bool> ExistsAsync();

    }
}
