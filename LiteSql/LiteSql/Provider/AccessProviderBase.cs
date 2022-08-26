﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// Access 数据库提供者基类
    /// </summary>
    public class AccessProviderBase : IProvider
    {
        #region OpenQuote 引号
        /// <summary>
        /// 引号
        /// </summary>
        public string OpenQuote
        {
            get
            {
                return "[";
            }
        }
        #endregion

        #region CloseQuote 引号
        /// <summary>
        /// 引号
        /// </summary>
        public string CloseQuote
        {
            get
            {
                return "]";
            }
        }
        #endregion

        #region 创建 DbConnection
        public virtual DbConnection CreateConnection(string connectionString) { return null; }
        #endregion

        #region 生成 DbCommand
        public virtual DbCommand GetCommand(DbConnection conn)
        {
            DbCommand command = conn.CreateCommand();
            return command;
        }
        #endregion

        #region 生成 DbCommand
        public virtual DbCommand GetCommand(string sql, DbConnection conn)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            return command;
        }
        #endregion

        #region 生成 DbParameter
        public virtual DbParameter GetDbParameter(string name, object value) { return null; }
        #endregion

        #region GetParameterMark
        public string GetParameterMark()
        {
            return "@";
        }
        #endregion

        #region 创建获取最大编号SQL
        public string CreateGetMaxIdSql(string tableName, string key)
        {
            return string.Format("SELECT Max({0}) FROM {1}", key, tableName);
        }
        #endregion

        #region 创建分页SQL
        public string CreatePageSql(string sql, string orderby, int pageSize, int currentPage)
        {
            StringBuilder sb = new StringBuilder();
            int startRow = 0;
            int endRow = 0;

            #region 分页查询语句
            endRow = pageSize * currentPage;
            //startRow = pageSize * currentPage > totalRows ? totalRows - pageSize * (currentPage - 1) : pageSize; //没有totalRows无法计算
            startRow = pageSize; //最后一页数据和前一页数据有重复，且数据始终为pageSize
            string[] orderbyArr = string.Format("{0} asc", orderby.Trim()).Split(' ');

            sb.AppendFormat(@"
                select * from(
                select top {4} * from 
                (select top {3} * from ({0}) order by {1} asc)
                order by {1} desc
                ) order by {1} {2}", sql, orderbyArr[0], orderbyArr[1], endRow, startRow);
            #endregion

            return sb.ToString();

        }
        #endregion

        #region ForContains
        public SqlValue ForContains(string value)
        {
            return new SqlValue(" '%' & {0} & '%' ", value);
        }
        #endregion

        #region ForStartsWith
        public SqlValue ForStartsWith(string value)
        {
            return new SqlValue(" {0} & '%' ", value);
        }
        #endregion

        #region ForEndsWith
        public SqlValue ForEndsWith(string value)
        {
            return new SqlValue(" '%' & {0} ", value);
        }
        #endregion

        #region ForDateTime
        public SqlValue ForDateTime(DateTime dateTime)
        {
            return new SqlValue(" CDate({0}) ", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        #endregion

        #region ForList
        public SqlValue ForList(IList list)
        {
            List<string> argList = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                argList.Add("@inParam" + i);
            }
            string args = string.Join(",", argList);

            return new SqlValue("(" + args + ")", list);
        }
        #endregion

    }
}
