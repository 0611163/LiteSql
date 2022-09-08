using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial class DBSession : ISession
    {
        #region SQL打印
        /// <summary>
        /// SQL打印
        /// </summary>
        public Action<string, DbParameter[]> OnExecuting { get; set; }
        #endregion

        #region  执行简单SQL语句

        #region Exists 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exists(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = ExecuteScalar(sqlString);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region ExistsAsync 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await ExecuteScalarAsync(sqlString);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion


        #region Execute 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int Execute(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(sqlString, _conn.Conn))
                {
                    if (_tran != null) cmd.Transaction = _tran.Tran;
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
            }
        }
        #endregion

        #region ExecuteAsync 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public async Task<int> ExecuteAsync(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            using (_conn = await DbConnectionFactory.GetConnectionAsync(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(sqlString, _conn.Conn))
                {
                    if (_tran != null) cmd.Transaction = _tran.Tran;
                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows;
                }
            }
        }
        #endregion


        #region ExecuteScalar 执行SQL语句，返回一个值
        /// <summary>
        /// 执行SQL语句，返回一个值
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public object ExecuteScalar(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(_conn.Conn))
                {
                    PrepareCommand(cmd, _conn.Conn, _tran, sqlString, null);
                    object result = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return result;
                }
            }
        }
        #endregion

        #region ExecuteScalarAsync 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public async Task<object> ExecuteScalarAsync(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            using (_conn = await DbConnectionFactory.GetConnectionAsync(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(_conn.Conn))
                {
                    await PrepareCommandAsync(cmd, _conn.Conn, _tran, sqlString, null);
                    var task = cmd.ExecuteScalarAsync();
                    object result = await task;
                    cmd.Parameters.Clear();
                    return result;
                }
            }
        }
        #endregion


        #region QuerySingle<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public T QuerySingle<T>(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);


            object obj = ExecuteScalar(sqlString);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingle 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public object QuerySingle(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = ExecuteScalar(sqlString);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion

        #region QuerySingleAsync<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public async Task<T> QuerySingleAsync<T>(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await ExecuteScalarAsync(sqlString);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingleAsync 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public async Task<object> QuerySingleAsync(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await ExecuteScalarAsync(sqlString);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion


        #region QueryCount 查询数量
        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql, int pageSize, out long pageCount)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = QueryCount(sql);
            pageCount = (count - 1) / pageSize + 1;
            return count;
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns>查询结果的数量</returns>
        public async Task<CountResult> QueryCountAsync(string sql, int pageSize)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = await QueryCountAsync(sql);
            long pageCount = (count - 1) / pageSize + 1;
            return new CountResult(count, pageCount);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);
            return QuerySingle<long>(sql);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>查询结果的数量</returns>
        public Task<long> QueryCountAsync(string sql)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);
            return QuerySingleAsync<long>(sql);
        }
        #endregion


        #region ExecuteReader 执行查询语句，返回IDataReader
        /// <summary>
        /// 执行查询语句，返回IDataReader ( 注意：调用该方法后，一定要对IDataReader进行Close )
        /// </summary>
        private DbDataReader ExecuteReader(string sqlString, DbConnectionExt conn)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            using (DbCommand cmd = _provider.GetCommand(sqlString, conn.Conn))
            {
                DbDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
        }
        #endregion

        #region ExecuteReaderAsync 执行查询语句，返回IDataReader
        /// <summary>
        /// 执行查询语句，返回IDataReader ( 注意：调用该方法后，一定要对IDataReader进行Close )
        /// </summary>
        private async Task<DbDataReader> ExecuteReaderAsync(string sqlString, DbConnectionExt conn)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            using (DbCommand cmd = _provider.GetCommand(sqlString, conn.Conn))
            {
                DbDataReader myReader = await cmd.ExecuteReaderAsync();
                return myReader;
            }
        }
        #endregion

        #endregion

        #region 执行带参数的SQL语句

        #region Execute 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>影响的记录数</returns>
        public int Execute(string SQLString, DbParameter[] cmdParms)
        {
            OnExecuting?.Invoke(SQLString, cmdParms);

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(_conn.Conn))
                {
                    PrepareCommand(cmd, _conn.Conn, _tran, SQLString, cmdParms);
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
            }
        }
        #endregion

        #region ExecuteAsync 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>影响的记录数</returns>
        public async Task<int> ExecuteAsync(string SQLString, DbParameter[] cmdParms)
        {
            OnExecuting?.Invoke(SQLString, cmdParms);

            using (_conn = await DbConnectionFactory.GetConnectionAsync(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(_conn.Conn))
                {
                    await PrepareCommandAsync(cmd, _conn.Conn, _tran, SQLString, cmdParms);
                    var task = cmd.ExecuteNonQueryAsync();
                    int rows = await task;
                    cmd.Parameters.Clear();
                    return rows;
                }
            }
        }
        #endregion


        #region ExecuteScalar 执行SQL语句，返回一个值
        /// <summary>
        /// 执行SQL语句，返回一个值
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>影响的记录数</returns>
        public object ExecuteScalar(string SQLString, DbParameter[] cmdParms)
        {
            OnExecuting?.Invoke(SQLString, cmdParms);

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(_conn.Conn))
                {
                    PrepareCommand(cmd, _conn.Conn, _tran, SQLString, cmdParms);
                    object result = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return result;
                }
            }
        }
        #endregion

        #region ExecuteScalarAsync 执行SQL语句，返回影响的记录数
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>影响的记录数</returns>
        public async Task<object> ExecuteScalarAsync(string SQLString, DbParameter[] cmdParms)
        {
            OnExecuting?.Invoke(SQLString, cmdParms);

            using (_conn = await DbConnectionFactory.GetConnectionAsync(_provider, _connectionString, _tran))
            {
                using (DbCommand cmd = _provider.GetCommand(_conn.Conn))
                {
                    await PrepareCommandAsync(cmd, _conn.Conn, _tran, SQLString, cmdParms);
                    var task = cmd.ExecuteScalarAsync();
                    object result = await task;
                    cmd.Parameters.Clear();
                    return result;
                }
            }
        }
        #endregion


        #region Exists 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exists(string sqlString, DbParameter[] cmdParms)
        {
            OnExecuting?.Invoke(sqlString, null);

            object obj = ExecuteScalar(sqlString, cmdParms);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region ExistsAsync 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await ExecuteScalarAsync(sqlString, cmdParms);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion


        #region QuerySingle<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public T QuerySingle<T>(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = ExecuteScalar(sqlString, cmdParms);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingle 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public object QuerySingle(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = ExecuteScalar(sqlString, cmdParms);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion

        #region QuerySingleAsync<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public async Task<T> QuerySingleAsync<T>(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await ExecuteScalarAsync(sqlString, cmdParms);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingleAsync 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public async Task<object> QuerySingleAsync(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await ExecuteScalarAsync(sqlString, cmdParms);

            if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion


        #region QueryCount 查询数量
        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql, DbParameter[] cmdParms, int pageSize, out long pageCount)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = QueryCount(sql, cmdParms);
            pageCount = (count - 1) / pageSize + 1;
            return count;
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns>查询结果的数量</returns>
        public async Task<CountResult> QueryCountAsync(string sql, DbParameter[] cmdParms, int pageSize)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = await QueryCountAsync(sql, cmdParms);
            long pageCount = (count - 1) / pageSize + 1;
            return new CountResult(count, pageCount);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql, DbParameter[] cmdParms)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);

            return QuerySingle<long>(sql, cmdParms);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>查询结果的数量</returns>
        public async Task<long> QueryCountAsync(string sql, DbParameter[] cmdParms)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);

            return await QuerySingleAsync<long>(sql, cmdParms);
        }
        #endregion


        #region ExecuteReader 执行查询语句，返回IDataReader
        /// <summary>
        /// 执行查询语句，返回IDataReader ( 注意：调用该方法后，一定要对IDataReader进行Close )
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        ///  <param name="cmdParms">参数</param>
        /// <returns>IDataReader</returns>
        private DbDataReader ExecuteReader(string sqlString, DbParameter[] cmdParms, DbConnectionExt conn)
        {
            OnExecuting?.Invoke(sqlString, cmdParms);

            using (DbCommand cmd = _provider.GetCommand(conn.Conn))
            {
                PrepareCommand(cmd, conn.Conn, null, sqlString, cmdParms);
                DbDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
        }
        #endregion

        #region ExecuteReaderAsync 执行查询语句，返回IDataReader
        /// <summary>
        /// 执行查询语句，返回IDataReader ( 注意：调用该方法后，一定要对IDataReader进行Close )
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        ///  <param name="cmdParms">参数</param>
        /// <returns>IDataReader</returns>
        private async Task<DbDataReader> ExecuteReaderAsync(string sqlString, DbParameter[] cmdParms, DbConnectionExt conn)
        {
            OnExecuting?.Invoke(sqlString, cmdParms);

            using (DbCommand cmd = _provider.GetCommand(conn.Conn))
            {
                await PrepareCommandAsync(cmd, conn.Conn, null, sqlString, cmdParms);
                DbDataReader myReader = await cmd.ExecuteReaderAsync();
                cmd.Parameters.Clear();
                return myReader;
            }
        }
        #endregion

        #region PrepareCommand
        private static void PrepareCommand(DbCommand cmd, DbConnection conn, DbTransactionExt trans, string cmdText, DbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null) cmd.Transaction = trans.Tran;
            cmd.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                foreach (DbParameter parm in cmdParms)
                {
                    cmd.Parameters.Add(parm);
                }
            }
        }
        #endregion

        #region PrepareCommandAsync
        private static async Task PrepareCommandAsync(DbCommand cmd, DbConnection conn, DbTransactionExt trans, string cmdText, DbParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null) cmd.Transaction = trans.Tran;
            cmd.CommandType = CommandType.Text;
            if (cmdParms != null)
            {
                foreach (DbParameter parm in cmdParms)
                {
                    cmd.Parameters.Add(parm);
                }
            }
        }
        #endregion

        #endregion

        #region 传SqlString

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sql">SqlString</param>
        /// <returns>影响的记录数</returns>
        public int Execute(ISqlString sql)
        {
            return Execute(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sql">SqlString</param>
        /// <returns>影响的记录数</returns>
        public Task<int> ExecuteAsync(ISqlString sql)
        {
            return ExecuteAsync(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exists(ISqlString sql)
        {
            return Exists(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        public Task<bool> ExistsAsync(ISqlString sql)
        {
            return ExistsAsync(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public object QuerySingle(ISqlString sql)
        {
            return QuerySingle(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public T QuerySingle<T>(ISqlString sql)
        {
            return QuerySingle<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public Task<object> QuerySingleAsync(ISqlString sql)
        {
            return QuerySingleAsync(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public Task<T> QuerySingleAsync<T>(ISqlString sql)
        {
            return QuerySingleAsync<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SqlString</param>
        /// <returns>数量</returns>
        public long QueryCount(ISqlString sql)
        {
            return QueryCount(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SqlString</param>
        /// <returns>数量</returns>
        public Task<long> QueryCountAsync(ISqlString sql)
        {
            return QueryCountAsync(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SqlString</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(ISqlString sql, int pageSize, out long pageCount)
        {
            return QueryCount(sql.SQL, sql.Params, pageSize, out pageCount);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SqlString</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns>查询结果的数量</returns>
        public Task<CountResult> QueryCountAsync(ISqlString sql, int pageSize)
        {
            return QueryCountAsync(sql.SQL, sql.Params, pageSize);
        }

        #endregion

    }
}
