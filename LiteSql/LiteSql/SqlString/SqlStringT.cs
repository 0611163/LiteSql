using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// SQL字符串类
    /// </summary>
    public class SqlString<T> : SqlString, ISqlQueryable<T> where T : new()
    {
        #region 构造函数
        /// <summary>
        /// SQL字符串类
        /// </summary>
        public SqlString(IProvider provider, IDBSession session, string sql = null, params object[] args)
            : base(provider, session, sql, args)
        {

        }
        #endregion

        #region Queryable
        /// <summary>
        /// 创建单表查询SQL
        /// </summary>
        /// <param name="alias">别名，默认值t</param>
        public ISqlQueryable<T> Queryable(string alias = null)
        {
            Type type = typeof(T);
            alias = alias ?? "t";

            _sql.AppendFormat("select ", _dbSession.GetTableName(_provider, type));

            PropertyInfoEx[] propertyInfoExArray = DBSession.GetEntityProperties(type);
            foreach (PropertyInfoEx propertyInfoEx in propertyInfoExArray)
            {
                PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;
                if (propertyInfo.GetCustomAttribute<ColumnAttribute>() != null)
                {
                    _sql.AppendFormat("{0}.{1}{2}{3},", alias, _provider.OpenQuote, propertyInfoEx.FieldName, _provider.CloseQuote);
                }
            }

            _sql.Remove(_sql.Length - 1, 1);

            _sql.AppendFormat(" from {0} {1}", _dbSession.GetTableName(_provider, type), alias);

            return this;
        }
        #endregion

        #region WhereIf
        /// <summary>
        /// 追加参数化查询条件SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="expression">Lambda 表达式</param>
        public ISqlQueryable<T> WhereIf(bool condition, Expression<Func<T, object>> expression)
        {
            if (condition)
            {
                Where(expression);
            }

            return this;
        }
        #endregion

        #region WhereIf
        /// <summary>
        /// 追加参数化查询条件SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="expression">Lambda 表达式</param>
        public ISqlQueryable<T> WhereIf<U>(bool condition, Expression<Func<U, object>> expression)
        {
            if (condition)
            {
                Where<U>(expression);
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化查询条件SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public ISqlQueryable<T> Where(Expression<Func<T, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " (" + condition.VisitLambda(expression, out dbParameters) + ")";

                if (dbParameters != null)
                {
                    result = ParamsAddRange(dbParameters, result);
                }

                if (RemoveSubSqls(_sql.ToString()).Contains(" where "))
                {
                    _sql.Append(" and " + result);
                }
                else
                {
                    _sql.Append(" where " + result);
                }
            }
            catch
            {
                throw;
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化查询条件SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public ISqlQueryable<T> Where<U>(Expression<Func<U, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " (" + condition.VisitLambda(expression, out dbParameters) + ")";

                if (dbParameters != null)
                {
                    result = ParamsAddRange(dbParameters, result);
                }

                if (RemoveSubSqls(_sql.ToString()).Contains(" where "))
                {
                    _sql.Append(" and " + result);
                }
                else
                {
                    _sql.Append(" where " + result);
                }
            }
            catch
            {
                throw;
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化查询条件SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public ISqlQueryable<T> Where<U>(Expression<Func<T, U, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " (" + condition.VisitLambda(expression, out dbParameters) + ")";

                if (dbParameters != null)
                {
                    result = ParamsAddRange(dbParameters, result);
                }

                if (RemoveSubSqls(_sql.ToString()).Contains(" where "))
                {
                    _sql.Append(" and " + result);
                }
                else
                {
                    _sql.Append(" where " + result);
                }
            }
            catch
            {
                throw;
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化查询条件SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public ISqlQueryable<T> Where<U, D>(Expression<Func<T, U, D, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " (" + condition.VisitLambda(expression, out dbParameters) + ")";

                if (dbParameters != null)
                {
                    result = ParamsAddRange(dbParameters, result);
                }

                if (RemoveSubSqls(_sql.ToString()).Contains(" where "))
                {
                    _sql.Append(" and " + result);
                }
                else
                {
                    _sql.Append(" where " + result);
                }
            }
            catch
            {
                throw;
            }

            return this;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// 追加 order by SQL
        /// </summary>
        public ISqlQueryable<T> OrderBy(Expression<Func<T, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.OrderBy);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            if (!_sql.ToString().Contains(" order by "))
            {
                _sql.AppendFormat(" order by {0} asc ", sql);
            }
            else
            {
                _sql.AppendFormat(", {0} asc ", sql);
            }

            return this;
        }
        #endregion

        #region OrderByDescending
        /// <summary>
        /// 追加 order by SQL
        /// </summary>
        public ISqlQueryable<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.OrderByDescending);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            if (!_sql.ToString().Contains(" order by "))
            {
                _sql.AppendFormat(" order by {0} desc ", sql);
            }
            else
            {
                _sql.AppendFormat(", {0} desc ", sql);
            }

            return this;
        }
        #endregion

        #region LeftJoin
        /// <summary>
        /// 追加 left join SQL
        /// </summary>
        public ISqlQueryable<T> LeftJoin<U>(Expression<Func<T, U, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.LeftJoin);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            string tableName = _dbSession.GetTableName(_provider, typeof(U));

            string alias = sql.Split('=')[1].Split('.')[0].Trim();

            _sql.AppendFormat(" left join {0} {1} on {2} ", tableName, alias, sql);

            return this;
        }
        #endregion

        #region InnerJoin
        /// <summary>
        /// 追加 inner join SQL
        /// </summary>
        public ISqlQueryable<T> InnerJoin<U>(Expression<Func<T, U, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.LeftJoin);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            string tableName = _dbSession.GetTableName(_provider, typeof(U));

            string alias = sql.Split('=')[1].Split('.')[0].Trim();

            _sql.AppendFormat(" inner join {0} {1} on {2} ", tableName, alias, sql);

            return this;
        }
        #endregion

        #region RightJoin
        /// <summary>
        /// 追加 right join SQL
        /// </summary>
        public ISqlQueryable<T> RightJoin<U>(Expression<Func<T, U, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.LeftJoin);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            string tableName = _dbSession.GetTableName(_provider, typeof(U));

            string alias = sql.Split('=')[1].Split('.')[0].Trim();

            _sql.AppendFormat(" right join {0} {1} on {2} ", tableName, alias, sql);

            return this;
        }
        #endregion

        #region WhereJoin
        /// <summary>
        /// Where 连表
        /// </summary>
        public ISqlQueryable<T> WhereJoin<U>(Expression<Func<T, U, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.LeftJoin);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            _sql.AppendFormat(" where {0} ", sql);

            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// 追加 select SQL
        /// </summary>
        /// <param name="subSql">子SQL</param>
        /// <param name="alias">别名，默认值t</param>
        public ISqlQueryable<T> Select(ISqlString subSql = null, string alias = null)
        {
            return Select(null, subSql, alias);
        }

        /// <summary>
        /// 追加 select SQL
        /// </summary>
        /// <param name="sql">SQL，插入到子SQL的前面，或者插入到{0}的位置</param>
        /// <param name="subSql">子SQL</param>
        /// <param name="alias">别名，默认值t</param>
        public ISqlQueryable<T> Select(string sql, ISqlString subSql = null, string alias = null)
        {
            alias = alias ?? "t";
            if (sql == null) sql = string.Empty;
            if (subSql == null) subSql = _session.CreateSql();
            if (sql.Contains("{0}"))
            {
                sql = sql.Replace("{0}", subSql.SQL);
            }
            else
            {
                sql = sql + subSql.SQL;
            }
            if (_sql.ToString().Contains(" from "))
            {
                string[] leftRigth = _sql.ToString().Split(new string[] { " from " }, StringSplitOptions.None);
                string left = leftRigth[0];
                string right = leftRigth[1];

                if (left.Trim().EndsWith("select"))
                {
                    _sql = new StringBuilder(string.Format("{0} {1} from {2}", left, sql, right));
                }
                else
                {
                    _sql = new StringBuilder(string.Format("{0}, {1} from {2}", left, sql, right));
                }
            }
            else
            {
                _sql = new StringBuilder(string.Format("select {0} from {1} {2}", sql, _dbSession.GetTableName(_provider, typeof(T)), alias));
            }

            string newSubSql = ParamsAddRange(subSql.Params, subSql.SQL);

            if (!string.IsNullOrWhiteSpace(newSubSql)
                && newSubSql.Contains("select ")
                && newSubSql.Contains(" from "))
            {
                _subSqls.Add(newSubSql);
            }

            return this;
        }

        /// <summary>
        /// 追加 select SQL
        /// </summary>
        /// <param name="expression">返回匿名对象的表达式</param>
        public ISqlQueryable<T> Select(Expression<Func<T, object>> expression)
        {
            Type type = expression.Body.Type;
            PropertyInfo[] props = type.GetProperties();
            Dictionary<string, string> dict = DBSession.GetEntityProperties(typeof(T)).ToLookup(a => a.PropertyInfo.Name).ToDictionary(a => a.Key, a => a.First().FieldName);
            int i = 0;
            StringBuilder fields = new StringBuilder();
            if (type != typeof(string))
            {
                foreach (PropertyInfo propInfo in props)
                {
                    i++;
                    fields.AppendFormat("{0}.{1}", expression.Parameters[0].Name, _provider.OpenQuote + dict[propInfo.Name] + _provider.CloseQuote);
                    if (i < props.Length) fields.Append(", ");
                }
            }
            else
            {
                if (expression.Body is ConstantExpression)
                {
                    fields.Append((expression.Body as ConstantExpression).Value.ToString());
                }
                else
                {
                    throw new Exception("不支持");
                }
            }

            if (_sql.ToString().Contains(" from "))
            {
                string[] leftRigth = _sql.ToString().Split(new string[] { " from " }, StringSplitOptions.None);
                string left = leftRigth[0];
                string right = leftRigth[1];

                if (left.Trim().EndsWith("select"))
                {
                    _sql = new StringBuilder(string.Format("{0} {1} from {2}", left, fields.ToString(), right));
                }
                else
                {
                    _sql = new StringBuilder(string.Format("{0}, {1} from {2}", left, fields.ToString(), right));
                }
            }
            else
            {
                _sql.AppendFormat("select {0} from {1} {2}", fields.ToString(), _dbSession.GetTableName(_provider, typeof(T)), expression.Parameters[0].Name);
            }

            return this;
        }

        /// <summary>
        /// 追加 select SQL
        /// </summary>
        /// <typeparam name="U">实体类型</typeparam>
        /// <param name="expression">属性名表达式</param>
        /// <param name="expression2">别名表达式</param>
        public ISqlQueryable<T> Select<U>(Expression<Func<U, object>> expression, Expression<Func<T, object>> expression2)
        {
            DbParameter[] dbParameters;

            ExpressionHelper<U> condition = new ExpressionHelper<U>(this, _provider, _dbParameterNames, SqlStringMethod.Select);
            string sql = condition.VisitLambda(expression, out dbParameters);

            ExpressionHelper<U> condition2 = new ExpressionHelper<U>(this, _provider, _dbParameterNames, SqlStringMethod.Select);
            string sql2 = condition.VisitLambda(expression2, out dbParameters);

            if (_sql.ToString().Contains(" from "))
            {
                string[] leftRigth = _sql.ToString().Split(new string[] { " from " }, StringSplitOptions.None);
                string left = leftRigth[0];
                string right = leftRigth[1];

                _sql = new StringBuilder(string.Format("{0}, {1} as {2} from {3}", left, sql, sql2.Split('.')[1].Trim(), right));
            }
            else
            {
                _sql = new StringBuilder(string.Format("select {0} as {1} from {2} {3}", sql, sql2.Split('.')[1].Trim(), _dbSession.GetTableName(_provider, typeof(T)), expression2.Parameters[0].Name));
            }

            return this;
        }
        #endregion

        #region ToList
        /// <summary>
        /// 执行查询
        /// </summary>
        public List<T> ToList()
        {
            return _session.QueryList<T>(this.SQL, this.Params);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public async Task<List<T>> ToListAsync()
        {
            return await _session.QueryListAsync<T>(this.SQL, this.Params);
        }
        #endregion

        #region ToPageList
        /// <summary>
        /// 执行查询
        /// </summary>
        public List<T> ToPageList(int page, int pageSize)
        {
            string ORDER_BY = " order by ";
            string[] strArr = this.SQL.Split(new string[] { ORDER_BY }, StringSplitOptions.None);
            string orderBy = strArr.Length > 1 ? ORDER_BY + strArr[1] : string.Empty;

            return _session.QueryPage<T>(strArr[0], orderBy, pageSize, page, this.Params);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public async Task<List<T>> ToPageListAsync(int page, int pageSize)
        {
            string ORDER_BY = " order by ";
            string[] strArr = this.SQL.Split(new string[] { ORDER_BY }, StringSplitOptions.None);
            string orderBy = strArr.Length > 1 ? ORDER_BY + strArr[1] : string.Empty;

            return await _session.QueryPageAsync<T>(strArr[0], orderBy, pageSize, page, this.Params);
        }
        #endregion

        #region Count
        /// <summary>
        /// 返回数量
        /// </summary>
        public long Count()
        {
            return _session.QueryCount(this.SQL, this.Params);
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        public async Task<long> CountAsync()
        {
            return await _session.QueryCountAsync(this.SQL, this.Params);
        }
        #endregion

        #region First
        /// <summary>
        /// 返回数量
        /// </summary>
        public T First()
        {
            return _session.QueryList<T>(this.SQL, this.Params).FirstOrDefault();
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        public async Task<T> FirstAsync()
        {
            return (await _session.QueryListAsync<T>(this.SQL, this.Params)).FirstOrDefault();
        }
        #endregion

        #region Exists
        /// <summary>
        /// 是否存在
        /// </summary>
        public new bool Exists()
        {
            return _session.Exists(this.SQL, this.Params);
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        public new async Task<bool> ExistsAsync()
        {
            return await _session.ExistsAsync(this.SQL, this.Params);
        }
        #endregion

        #region Delete
        /// <summary>
        /// 删除
        /// </summary>
        public int Delete()
        {
            string[] sqlParts = this.SQL.Split(new string[] { " where " }, StringSplitOptions.None);
            string right;
            if (sqlParts.Length > 1)
            {
                right = sqlParts[1];
            }
            else
            {
                right = sqlParts[0];
            }

            Regex regex = new Regex("[\\(]?[\\s]*([\\w]+\\.)", RegexOptions.IgnoreCase);
            Match match = regex.Match(right);
            if (match.Success)
            {
                right = right.Replace(match.Groups[1].Value, " ");
            }

            return _session.DeleteByCondition<T>(right, this.Params);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public Task<int> DeleteAsync()
        {
            string[] sqlParts = this.SQL.Split(new string[] { " where " }, StringSplitOptions.None);
            string right;
            if (sqlParts.Length > 1)
            {
                right = sqlParts[1];
            }
            else
            {
                right = sqlParts[0];
            }

            Regex regex = new Regex("[\\(]?[\\s]*([\\w]+\\.)", RegexOptions.IgnoreCase);
            Match match = regex.Match(right);
            if (match.Success)
            {
                right = right.Replace(match.Groups[1].Value, " ");
            }

            return _session.DeleteByConditionAsync<T>(right, this.Params);
        }
        #endregion

    }
}
