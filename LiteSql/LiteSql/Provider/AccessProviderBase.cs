﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// Access 数据库提供者基类
    /// </summary>
    public abstract class AccessProviderBase : IProvider
    {
        #region OpenQuote 引号
        /// <summary>
        /// 引号
        /// </summary>
        public virtual string OpenQuote
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
        public virtual string CloseQuote
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

        #region GetParameterName
        public virtual string GetParameterName(string parameterName, Type parameterType)
        {
            return "@" + parameterName;
        }
        #endregion

        #region 创建获取最大编号SQL
        public virtual string CreateGetMaxIdSql(string tableName, string key)
        {
            return string.Format("SELECT Max({0}) FROM {1}", key, tableName);
        }
        #endregion

        #region 创建分页SQL
        public virtual string CreatePageSql(string sql, string orderby, int pageSize, int currentPage)
        {
            if (string.IsNullOrWhiteSpace(orderby)) throw new Exception("Access数据库分页查询必须有order by子句");

            Regex regex = new Regex("order[\\s]+by", RegexOptions.IgnoreCase);
            orderby = regex.Replace(orderby, string.Empty);
            string[] orderbyArray = orderby.Split(',');

            StringBuilder sb = new StringBuilder();
            int startRow = 0;
            int endRow = 0;

            #region 分页查询语句
            endRow = pageSize * currentPage;
            //startRow = pageSize * currentPage > totalRows ? totalRows - pageSize * (currentPage - 1) : pageSize; //没有totalRows无法计算
            startRow = pageSize; //最后一页数据和前一页数据有重复，且数据始终为pageSize
            string[] firstOrderbyArray = orderbyArray[0].Trim().Split(' ');

            sb.AppendFormat(@"
                select * from(
                select top {4} * from 
                (select top {3} * from ({0}) order by {1} asc)
                order by {1} desc
                ) order by {2}", sql, firstOrderbyArray[0], orderby, endRow, startRow);
            #endregion

            return sb.ToString();

        }
        #endregion

        #region 删除SQL语句模板
        /// <summary>
        /// 删除SQL语句模板 两个值分别对应 “delete from [表名] where [查询条件]”中的“delete from”和“where”
        /// </summary>
        public virtual Tuple<string, string> CreateDeleteSqlTempldate()
        {
            return new Tuple<string, string>("delete from", "where");
        }
        #endregion

        #region 更新SQL语句模板
        /// <summary>
        /// 更新SQL语句模板 三个值分别对应 “update [表名] set [赋值语句] where [查询条件]”中的“update”、“set”和“where”
        /// </summary>
        public virtual Tuple<string, string, string> CreateUpdateSqlTempldate()
        {
            return new Tuple<string, string, string>("update", "set", "where");
        }
        #endregion

        #region ForList
        public virtual SqlValue ForList(IList list)
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
