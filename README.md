# LiteSql

## 简介

一款使用原生SQL查询的轻量级ORM，单表查询和SQL拼接查询条件支持Lambda表达式。支持Oracle、MSSQL、MySQL、PostgreSQL、SQLite、Access、ClickHouse等数据库。

## 经典示例

```C#
DateTime? startTime = null;

var session = LiteSqlFactory.GetSession();

session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

List<SysUser> list = session.CreateSql(@"
    select * from sys_user t where t.id <= @Id", new { Id = 20 })

    .Append(@" and t.create_userid = @CreateUserId 
        and t.password like @Password
        and t.id in @Ids",
        new
        {
            CreateUserId = "1",
            Password = "%345%",
            Ids = session.ForList(new List<int> { 1, 2, 9, 10, 11 })
        })

    .AppendIf(startTime.HasValue, " and t.create_time >= @StartTime ", new { StartTime = startTime })

    .Append(" and t.create_time <= @EndTime ", new { EndTime = new DateTime(2022, 8, 1) })

    .QueryList<SysUser>();

long id = session.CreateSql("select id from sys_user where id=@Id", new { Id = 1 })
    .QuerySingle<long>();
Assert.IsTrue(id == 1);

foreach (SysUser item in list)
{
    Console.WriteLine(ModelToStringUtil.ToString(item));
}
Assert.IsTrue(list.Count > 0);
```

## 经典示例2：
```C#
var session = LiteSqlFactory.GetSession();
List<BsOrder> list = session
    .Sql<BsOrder>(@"
        select o.*, u.user_name as OrderUserName, u.real_name as OrderUserRealName 
        from bs_order o
        left join sys_user u on u.id=o.order_userid")
    .Where(o => o.Status == 0
        && o.Remark.Contains("订单")
        && o.OrderTime >= new DateTime(2010, 1, 1)
        && o.OrderTime < DateTime.Now.Date.AddDays(1))
    .Where<SysUser>(u => new long[] { 8, 9, 10 }.Contains(u.Id))
    .Append("order by o.order_time desc, o.id asc")
    .ToList();
```

## 特点

1. 支持Oracle、SQL Server、MySQL、PostgreSQL、SQLite五种数据库；另外只要ADO.NET支持的数据库，都可以很方便地通过实现IProvider接口支持，仅需写150行左右的代码
2. 有配套的Model生成器
3. 数据插入、更新、批量插入、批量更新，支持实体类、实体类集合，无需拼SQL；删除操作支持根据主键或查询条件删除；增删改支持联合主键
4. 查询以原生SQL为主，查询Where条件可以拼接Lambda表达式
5. 支持参数化查询，统一不同数据库的参数化查询SQL
6. 支持连接多个数据源
9. 支持手动分表
10. 单表查询支持Lambda表达式

## 优点

1. 比较简单，学习成本低
2. 查询以原生SQL为主，Lambda表达式辅助
3. 代码量仅4000多行，更容易修改和掌控代码质量

## 缺点

1. 对Lambda表达式的支持比较弱
2. 复杂查询不支持Lambda表达式(连表查询、子查询、分组统计查询、嵌套查询等不支持Lambda表达式写法)

## 建议

1. 单表查询、简单的连表查询可以使用Lambda表达式
2. 复杂查询建议使用原生SQL
3. 如果出现不支持的Lambda表达式写法，请使用原生SQL替代

## 开发环境

1. VS2022
2. 目标框架：net45;netstandard2.0;net6.0
3. 测试工程使用.NET Framework 4.5.2

## 配套Model生成器地址：

[https://gitee.com/s0611163/ModelGenerator](https://gitee.com/s0611163/ModelGenerator)

## Dapper版

使用ADO.NET操作数据库改成了使用Dapper操作数据库

[https://gitee.com/s0611163/Dapper.LiteSql/](https://gitee.com/s0611163/Dapper.LiteSql/)

## 支持 ClickHouse 数据库

[https://gitee.com/s0611163/ClickHouseTest](https://gitee.com/s0611163/ClickHouseTest)

这是一个示例，只要ADO.NET支持的数据库，您都可以通过实现IProvider接口尝试支持

## .NET 6 环境下测试

[https://gitee.com/s0611163/LiteSqlTest](https://gitee.com/s0611163/LiteSqlTest)

## 作者邮箱

    651029594@qq.com

## 使用步骤

1. 安装LiteSql

```text
Install-Package LiteSql -Version 1.5.19
```

2. 安装对应的数据库引擎

```text
Install-Package MySql.Data -Version 6.9.12
```

3. 实现对应的数据库Provider

注意：各实现方法一定要加上override关键字以重写基类的方法

```C#
using LiteSql;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DAL
{
    public class MySQLProvider : MySQLProviderBase, IDBProvider
    {
        #region 创建 DbConnection
        public override DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
        #endregion

        #region 生成 DbParameter
        public override DbParameter GetDbParameter(string name, object value)
        {
            return new MySqlParameter(name, value);
        }
        #endregion

    }
}

```

4. 定义LiteSqlFactory类

```C#
using LiteSql;
using System.Configuration;
using System.Threading.Tasks;

namespace DAL
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _liteSqlClient = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.MySQL, new MySQLProvider());
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static ISession GetSession(SplitTableMapping splitTableMapping = null)
        {
            return _liteSqlClient.GetSession(splitTableMapping);
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static async Task<ISession> GetSessionAsync(SplitTableMapping splitTableMapping = null)
        {
            return await _liteSqlClient.GetSessionAsync(splitTableMapping);
        }
        #endregion

    }
}
```

5. 依赖注入
```C#
var builder = WebApplication.CreateBuilder(args);

var db = new LiteSqlClient(builder.Configuration.GetConnectionString("DefaultConnection"), new MySQLProvider());
var secondDB = new LiteSqlClient<SecondDbFlag>(builder.Configuration.GetConnectionString("SecondConnection"), new MySQLProvider());

// Add services to the container.
// 注册数据库ILiteSqlClient
builder.Services.AddSingleton<ILiteSqlClient>(db);
// 注册第二个数据库ILiteSqlClient
builder.Services.AddSingleton<ILiteSqlClient<SecondDbFlag>>(secondDB);
// 注册数据库DbSession
builder.Services.AddScoped<IDbSession>(serviceProvider =>
{
    return serviceProvider.GetService<ILiteSqlClient>().GetSession();
});
// 注册第二个数据库DbSession
builder.Services.AddScoped<IDbSession<SecondDbFlag>>(serviceProvider =>
{
    return serviceProvider.GetService<ILiteSqlClient<SecondDbFlag>>().GetSession();
});

/// <summary>
/// 第二个数据库标识
/// </summary>
public class SecondDbFlag { }
```

## 配套Model生成器

### 使用Model生成器生成实体类

1. 实体类放在Models文件夹中
2. 扩展实体类放在ExtModels文件夹中
3. 实体类和扩展实体类使用partial修饰，实际上是一个类，放在不同的文件中
4. 如果需要添加自定义属性，请修改ExtModels，不要修改Models

#### 实体类示例

```C#
/// <summary>
/// 订单表
/// </summary>
[Serializable]
[Table("bs_order")]
public partial class BsOrder
{

    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    public string Id { get; set; }

    /// <summary>
    /// 订单时间
    /// </summary>
    [Column("order_time")]
    public DateTime OrderTime { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// 下单用户
    /// </summary>
    [Column("order_userid")]
    public long OrderUserid { get; set; }

    /// <summary>
    /// 订单状态(0草稿 1已下单 2已付款 3已发货 4完成)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    [Column("create_userid")]
    public string CreateUserid { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("create_time")]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    [Column("update_userid")]
    public string UpdateUserid { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column("update_time")]
    public DateTime? UpdateTime { get; set; }

}
```

### 修改扩展实体类

1. 修改扩展实体类，添加自定义属性
2. 下面的扩展实体类中，查询时OrderUserRealName会被自动填充，查询SQL：select t.*, u.real_name as OrderUserRealName from ......
3. DetailList不会被自动填充，需要手动查询

#### 扩展实体类示例

```C#
/// <summary>
/// 订单表
/// </summary>
public partial class BsOrder
{
    /// <summary>
    /// 订单明细集合
    /// </summary>
    [NotMapped]
    public List<BsOrderDetail> DetailList { get; set; }

    /// <summary>
    /// 下单用户姓名
    /// </summary>
    [NotMapped]
    public string OrderUserRealName { get; set; }

    /// <summary>
    /// 下单用户名
    /// </summary>
    [NotMapped]
    public string OrderUserName { get; set; }
}
```

## 增删改查示例

### 添加

```C#
public void Insert(SysUser info)
{
    var session = LiteSqlFactory.GetSession();
    session.Insert(info);  
}
```

### 添加并返回ID

```C#
public void Insert(SysUser info)
{
    var session = LiteSqlFactory.GetSession();
    long id = session.InsertReturnId(info, "select @@IDENTITY");   
}
```

### 批量添加

```C#
public void Insert(List<SysUser> list)
{
    var session = LiteSqlFactory.GetSession();
    session.Insert(list);   
}
```

### 修改

```C#
public void Update(SysUser info)
{
    var session = LiteSqlFactory.GetSession();
    session.Update(info);
}
```

### 批量修改

```C#
public void Update(List<SysUser> list)
{
    var session = LiteSqlFactory.GetSession();
    session.Update(list);
}
```

### 修改时只更新数据有变化的字段

```C#
var session = LiteSqlFactory.GetSession();

session.AttachOld(user); //附加更新前的旧数据，只更新数据发生变化的字段，提升更新性能

user.UpdateUserid = "1";
user.Remark = "测试修改用户" + _rnd.Next(1, 100);
user.UpdateTime = DateTime.Now;

session.Update(user);
```

```C#
var session = LiteSqlFactory.GetSession();

session.AttachOld(userList); //附加更新前的旧数据，只更新数据发生变化的字段，提升更新性能

foreach (SysUser user in userList)
{
    user.Remark = "测试修改用户" + _rnd.Next(1, 10000);
    user.UpdateUserid = "1";
    user.UpdateTime = DateTime.Now;
}

session.Update(userList);
```

### 删除


```C#
public void Delete(string id)
{
    var session = LiteSqlFactory.GetSession();
    session.DeleteById<SysUser>(id);
}
```

### 条件删除

```C#
var session = LiteSqlFactory.GetSession();
session.CreateSql("id>@Id", 20).Delete<SysUser>();
```

```C#
var session = LiteSqlFactory.GetSession();
session.Queryable<SysUser>().Where(t => t.Id > 20).Delete();
```

### 查询单个记录

```C#
public SysUser Get(string id)
{
    var session = LiteSqlFactory.GetSession();
    return session.QueryById<SysUser>(id);
}
```

```C#
var session = LiteSqlFactory.GetSession();
SysUser user = session.Query<SysUser>("select * from sys_user");
```

### 简单查询

```C#
var session = LiteSqlFactory.GetSession();
string sql = "select * from CARINFO_MERGE";
List<CarinfoMerge> result = session.QueryList<CarinfoMerge>(sql);
```

### 条件查询

```C#
public List<BsOrder> GetList(int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    var session = LiteSqlFactory.GetSession();

    ISqlString sql = session.CreateSql(@"
        select t.*, u.real_name as OrderUserRealName
        from bs_order t
        left join sys_user u on t.order_userid=u.id
        where 1=1");

    sql.AppendIf(status.HasValue, " and t.status=@status", status);

    sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like @remark", "%" + remark + "%");

    sql.AppendIf(startTime.HasValue, " and t.order_time >= @startTime ", startTime);

    sql.AppendIf(endTime.HasValue, " and t.order_time <= @endTime ", endTime);

    sql.Append(" order by t.order_time desc, t.id asc ");

    List<BsOrder> list = session.QueryList<BsOrder>(sql);
    return list;
}
```

### 条件查询(SQL参数支持匿名对象)

```C#
public List<BsOrder> GetList(int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    var session = LiteSqlFactory.GetSession();

    ISqlString sql = session.CreateSql(@"
        select t.*, u.real_name as OrderUserRealName
        from bs_order t
        left join sys_user u on t.order_userid=u.id
        where 1=1");

    sql.AppendIf(status.HasValue, " and t.status=@Status", new { Status = status });

    sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like @Remark", new { Remark = "%" + remark + "%" });

    sql.AppendIf(startTime.HasValue, " and t.order_time >= @StartTime ", new { StartTime = startTime } });

    sql.AppendIf(endTime.HasValue, " and t.order_time <= @EndTime ", endTime });

    sql.Append(" order by t.order_time desc, t.id asc ");

    List<BsOrder> list = session.QueryList<BsOrder>(sql);
    return list;
}
```

### 分页查询

```C#
public List<BsOrder> GetListPage(ref PageModel pageModel, int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    var session = LiteSqlFactory.GetSession();

    ISqlString sql = session.CreateSql(@"
        select t.*, u.real_name as OrderUserRealName
        from bs_order t
        left join sys_user u on t.order_userid=u.id
        where 1=1");

    sql.AppendIf(status.HasValue, " and t.status=@status", status);

    sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like @remark", "%" + remark + "%");

    sql.AppendIf(startTime.HasValue, " and t.order_time >= @startTime ", startTime);

    sql.AppendIf(endTime.HasValue, " and t.order_time <= @endTime ", endTime);

    string orderby = " order by t.order_time desc, t.id asc ";
    
    pageModel.TotalRows = session.QueryCount(sql);
    return session.QueryPage<BsOrder>(sql, orderby, pageModel.PageSize, pageModel.CurrentPage);
}
```


### 事务

```C#
public string Insert(BsOrder order, List<BsOrderDetail> detailList)
{
    var session = LiteSqlFactory.GetSession();

    try
    {
        session.BeginTransaction();

        order.Id = Guid.NewGuid().ToString("N");
        order.CreateTime = DateTime.Now;

        decimal amount = 0;
        foreach (BsOrderDetail detail in detailList)
        {
            detail.Id = Guid.NewGuid().ToString("N");
            detail.OrderId = order.Id;
            detail.CreateTime = DateTime.Now;
            amount += detail.Price * detail.Quantity;
            session.Insert(detail);
        }
        order.Amount = amount;

        session.Insert(order);

        session.CommitTransaction();

        return order.Id;
    }
    catch (Exception ex)
    {
        session.RollbackTransaction();
        Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
        throw ex;
    }
}
```

### 异步查询

```C#
public async Task<List<BsOrder>> GetListPageAsync(PageModel pageModel, int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    var session = LiteSqlFactory.GetSession();

    ISqlString sql = session.CreateSql(@"
        select t.*, u.real_name as OrderUserRealName
        from bs_order t
        left join sys_user u on t.order_userid=u.id
        where 1=1");

    sql.AppendIf(status.HasValue, " and t.status=@status", status);

    sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like @remark", "%" + remark + "%");

    sql.AppendIf(startTime.HasValue, " and t.order_time >= @startTime ", startTime);

    sql.AppendIf(endTime.HasValue, " and t.order_time <= @endTime ", endTime);

    string orderby = " order by t.order_time desc, t.id asc ";
    
    var countResult = await session.QueryCountAsync(sql, pageModel.PageSize);
    pageModel.TotalRows = countResult.Count;
    return await session.QueryPageAsync<BsOrder>(sql, orderby, pageModel.PageSize, pageModel.CurrentPage);
}
```

### 条件查询(使用 ForList 辅助方法)

```C#
public List<BsOrder> GetListExt(int? status, string remark, DateTime? startTime, DateTime? endTime, string ids)
{
    var session = LiteSqlFactory.GetSession();

    ISqlString sql = session.CreateSql(@"
        select t.*, u.real_name as OrderUserRealName
        from bs_order t
        left join sys_user u on t.order_userid=u.id
        where 1=1");

    sql.AppendIf(status.HasValue, " and t.status=@status", status);

    sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like @remark", "%" + remark + "%");

    sql.AppendIf(startTime.HasValue, " and t.order_time >= @startTime ", startTime);

    sql.AppendIf(endTime.HasValue, " and t.order_time <= @endTime ", endTime);

    sql.Append(" and t.id in @ids ", sql.ForList(ids.Split(',').ToList()));

    sql.Append(" order by t.order_time desc, t.id asc ");

    List<BsOrder> list = session.QueryList<BsOrder>(sql);
    return list;
}
```

### 使用Lambda表达式单表查询

单表分页查询使用ToPageList替换ToList即可

```C#
public void TestQueryByLambda6()
{
    var session = LiteSqlFactory.GetSession();

    ISqlQueryable<BsOrder> sql = session.Queryable<BsOrder>();

    string remark = "测试";

    List<BsOrder> list = sql.WhereIf(!string.IsNullOrWhiteSpace(remark),
        t => t.Remark.Contains(remark)
        && t.CreateTime < DateTime.Now
        && t.CreateUserid == "10")

        .OrderByDescending(t => t.OrderTime).OrderBy(t => t.Id)
        .ToList();

    foreach (BsOrder item in list)
    {
        Console.WriteLine(ModelToStringUtil.ToString(item));
    }
}
```

## 手动分表

### 定义LiteSqlFactory类

```C#
using LiteSql;
using System.Configuration;
using System.Threading.Tasks;

namespace DAL
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _liteSqlClient = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.MySQL, new MySQLProvider());
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static ISession GetSession(SplitTableMapping splitTableMapping = null)
        {
            return _liteSqlClient.GetSession(splitTableMapping);
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static async Task<ISession> GetSessionAsync(SplitTableMapping splitTableMapping = null)
        {
            return await _liteSqlClient.GetSessionAsync(splitTableMapping);
        }
        #endregion

    }
}
```

### 数据插入

```C#
var session = LiteSqlFactory.GetSession();
session.SetTableNameMap<SysUser>("sys_user_202208");

session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

session.Insert(user);

user.Id = session.QuerySingle<long>("select @@IDENTITY");
Console.WriteLine("插入成功, user.Id=" + user.Id);
```

### 数据更新

```C#
var session = LiteSqlFactory.GetSession();
session.SetTableNameMap<SysUser>("sys_user_202208");

session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

session.AttachOld(user); //附加更新前的旧数据，只更新数据发生变化的字段，提升更新性能

user.UpdateUserid = "1";
user.Remark = "测试修改分表数据" + _rnd.Next(1, 100);
user.UpdateTime = DateTime.Now;

session.Update(user);
```

### 数据删除

```C#
var session = LiteSqlFactory.GetSession();
session.SetTableNameMap<SysUser>("sys_user_202208");

session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

int deleteCount = session.DeleteByCondition<SysUser>(string.Format("id>20"));
Console.WriteLine(deleteCount + "条数据已删除");
int deleteCount2 = session.DeleteById<SysUser>(10000);
Console.WriteLine(deleteCount2 + "条数据已删除");
```

### 数据查询

```C#
var session = LiteSqlFactory.GetSession();
session.SetTableNameMap<SysUser>("sys_user_202208");

session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

ISqlQueryable<SysUser> sql = session.Queryable<SysUser>();

List<SysUser> list = sql.Where(t => t.Id < 10)
    .OrderBy(t => t.Id)
    .ToList();
```

## 支持更多数据库

    只要ADO.NET支持的数据库，都可以支持

### 如何实现

    以PostgreSQL为例，假如该库尚未支持PostgreSQL

#### 定义一个数据库提供者类，实现IProvider接口

```C#
using LiteSql;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace PostgreSQLTest
{
    public class PostgreSQLProvider : IProvider
    {
        #region OpenQuote 引号
        /// <summary>
        /// 引号
        /// </summary>
        public string OpenQuote
        {
            get
            {
                return "\"";
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
                return "\"";
            }
        }
        #endregion

        #region 生成 DbCommand
        public DbCommand GetCommand(DbConnection conn)
        {
            DbCommand command = conn.CreateCommand();
            return command;
        }
        #endregion

        #region 生成 DbCommand
        public DbCommand GetCommand(string sql, DbConnection conn)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            return command;
        }
        #endregion

        #region 创建 DbConnection
        public DbConnection CreateConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
        #endregion

        #region 生成 DbParameter
        public DbParameter GetDbParameter(string name, object value)
        {
            return new NpgsqlParameter(name, value);
        }
        #endregion

        #region GetParameterName
        public string GetParameterName(string parameterName, Type parameterType)
        {
            return "@" + parameterName;
        }
        #endregion

        #region 创建获取最大编号SQL
        public string CreateGetMaxIdSql(string key, Type type)
        {
            return string.Format("SELECT Max({0}) FROM {1}", key, type.Name);
        }
        #endregion

        #region 创建分页SQL
        public string CreatePageSql(string sql, string orderby, int pageSize, int currentPage, int totalRows)
        {
            StringBuilder sb = new StringBuilder();
            int startRow = 0;
            int endRow = 0;

            #region 分页查询语句
            startRow = pageSize * (currentPage - 1);

            sb.Append("select * from (");
            sb.Append(sql);
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                sb.Append(" ");
                sb.Append(orderby);
            }
            sb.AppendFormat(" ) row_limit limit {0} offset {1}", pageSize, startRow);
            #endregion

            return sb.ToString();
        }
        #endregion

        #region 删除SQL语句模板
        /// <summary>
        /// 删除SQL语句模板 两个值分别对应 “delete from [表名] where [查询条件]”中的“delete from”和“where”
        /// </summary>
        public Tuple<string, string> CreateDeleteSqlTempldate()
        {
            return new Tuple<string, string>("delete from", "where");
        }
        #endregion

        #region 更新SQL语句模板
        /// <summary>
        /// 更新SQL语句模板 三个值分别对应 “update [表名] set [赋值语句] where [查询条件]”中的“update”、“set”和“where”
        /// </summary>
        public Tuple<string, string, string> CreateUpdateSqlTempldate()
        {
            return new Tuple<string, string, string>("update", "set", "where");
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
```

#### 定义LiteSqlFactory类

    下面代码是.NET 5下的代码

```C#
using LiteSql;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace PostgreSQLTest
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _liteSqlClient;
        #endregion

        #region 静态构造函数
        static LiteSqlFactory()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            _liteSqlClient = new LiteSqlClient(connectionString, typeof(PostgreSQLProvider), new PostgreSQLProvider());
        }
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static ISession GetSession(SplitTableMapping splitTableMapping = null)
        {
            return _liteSqlClient.GetSession(splitTableMapping);
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static async Task<ISession> GetSessionAsync(SplitTableMapping splitTableMapping = null)
        {
            return await _liteSqlClient.GetSessionAsync(splitTableMapping);
        }
        #endregion

    }
}
```

    然后就可以使用了

### 支持ClickHouse

#### 定义ClickHouseProvider类实现IProvider接口

```C#
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using LiteSql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql.Provider
{
    public class ClickHouseProvider : IProvider
    {
        #region Quote
        public string OpenQuote
        {
            get
            {
                return "\"";
            }
        }

        public string CloseQuote
        {
            get
            {
                return "\"";
            }
        }
        #endregion

        #region 创建Db对象
        public DbConnection CreateConnection(string connectionString)
        {
            return new ClickHouseConnection(connectionString);
        }

        public DbCommand GetCommand(DbConnection conn)
        {
            DbCommand command = conn.CreateCommand();
            return command;
        }

        public DbCommand GetCommand(string sql, DbConnection conn)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            return command;
        }

        public DbParameter GetDbParameter(string name, object value)
        {
            DbParameter parameter = new ClickHouseDbParameter();
            parameter.ParameterName = name.Trim(new char[] { '{', '}' }).Split(':')[0];
            parameter.Value = value;
            DbType dbType = ColumnTypeUtil.GetDBType(value);
            parameter.DbType = dbType;
            return parameter;
        }
        #endregion

        #region Create SQL
        public string CreateGetMaxIdSql(string tableName, string key)
        {
            return string.Format("SELECT Max({0}) FROM {1}", key, tableName);
        }

        public string CreatePageSql(string sql, string orderby, int pageSize, int currentPage)
        {
            StringBuilder sb = new StringBuilder();
            int startRow = 0;
            int endRow = 0;

            #region 分页查询语句
            startRow = pageSize * (currentPage - 1);

            sb.Append("select * from (");
            sb.Append(sql);
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                sb.Append(" ");
                sb.Append(orderby);
            }
            sb.AppendFormat(" ) row_limit limit {0},{1}", startRow, pageSize);
            #endregion

            return sb.ToString();
        }
        #endregion

        #region 删除SQL语句模板
        /// <summary>
        /// 删除SQL语句模板 两个值分别对应 “delete from [表名] where [查询条件]”中的“delete from”和“where”
        /// </summary>
        public Tuple<string, string> CreateDeleteSqlTempldate()
        {
            return new Tuple<string, string>("alter table", "delete where");
        }
        #endregion

        #region 更新SQL语句模板
        /// <summary>
        /// 更新SQL语句模板 三个值分别对应 “update [表名] set [赋值语句] where [查询条件]”中的“update”、“set”和“where”
        /// </summary>
        public Tuple<string, string, string> CreateUpdateSqlTempldate()
        {
            return new Tuple<string, string, string>("alter table", "update", "where");
        }
        #endregion

        #region GetParameterName
        public string GetParameterName(string parameterName, Type parameterType)
        {
            return "{" + parameterName + ":" + parameterType.Name + "}";
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
```

#### ColumnTypeUtil工具类

```C#
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql.Provider
{
    public class ColumnTypeUtil
    {
        public static DbType GetDBType(object value)
        {
            if (value != null)
            {
                Type type = value.GetType();
                if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    return DbType.DateTime;
                }
                else if (type == typeof(string))
                {
                    return DbType.String;
                }
                else if (type == typeof(float) || type == typeof(float?))
                {
                    return DbType.Double;
                }
                else if (type == typeof(double) || type == typeof(double?))
                {
                    return DbType.Double;
                }
                else if (type == typeof(decimal) || type == typeof(decimal?))
                {
                    return DbType.Decimal;
                }
                else if (type == typeof(short) || type == typeof(short?))
                {
                    return DbType.Int16;
                }
                else if (type == typeof(int) || type == typeof(int?))
                {
                    return DbType.Int32;
                }
                else if (type == typeof(long) || type == typeof(long?))
                {
                    return DbType.Int64;
                }
            }
            return DbType.String;
        }

        public static string GetDBTypeName(Type parameterType)
        {
            if (parameterType == typeof(DateTime) || parameterType == typeof(DateTime?))
            {
                return "DateTime";
            }
            else if (parameterType == typeof(string))
            {
                return "String";
            }
            else if (parameterType == typeof(float) || parameterType == typeof(float?))
            {
                return "Float32";
            }
            else if (parameterType == typeof(double) || parameterType == typeof(double?))
            {
                return "Float64";
            }
            else if (parameterType == typeof(decimal) || parameterType == typeof(decimal?))
            {
                return "Float64";
            }
            else if (parameterType == typeof(short) || parameterType == typeof(short?))
            {
                return "Int16";
            }
            else if (parameterType == typeof(int) || parameterType == typeof(int?))
            {
                return "Int32";
            }
            else if (parameterType == typeof(long) || parameterType == typeof(long?))
            {
                return "Int64";
            }
            return "String";
        }

    }
}
```

#### 定义LiteSqlFactory

```C#
using LiteSql;
using LiteSql.Provider;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickHouseTest
{
    public class LiteSqlFactory
    {
        #region 变量
        private static ILiteSqlClient _liteSqlClient;
        #endregion

        #region 静态构造函数
        static LiteSqlFactory()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            _liteSqlClient = new LiteSqlClient(connectionString, typeof(ClickHouseProvider), new ClickHouseProvider());
        }
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static ISession GetSession(SplitTableMapping splitTableMapping = null)
        {
            return _liteSqlClient.GetSession(splitTableMapping);
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        /// <param name="splitTableMapping">分表映射</param>
        public static async Task<ISession> GetSessionAsync(SplitTableMapping splitTableMapping = null)
        {
            return await _liteSqlClient.GetSessionAsync(splitTableMapping);
        }
        #endregion

    }
}
```

#### 实体类

```C#
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    [Table("people_face_replica")]
    public class PeopleFace
    {
        [Column("captured_time")]
        public DateTime CapturedTime { get; set; }

        [Key]
        [Column("camera_id")]
        public string CameraId { get; set; }

        [Column("camera_fun_type")]
        public string CameraFunType { get; set; }

        [Key]
        [Column("face_id")]
        public string FaceId { get; set; }

        [Column("extra_info")]
        public string ExtraInfo { get; set; }

        [Column("event")]
        public string Event { get; set; }

        [Column("data_source3")]
        public string DataSource3 { get; set; }

        [Column("panoramic_image_url")]
        public string PanoramicImageUrl { get; set; }

        [Column("portrait_image_url")]
        public string PortraitImageUrl { get; set; }

    }
}
```

#### config.json文件

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Database=default;Username=default;Password=;Host=192.168.120.130;Port=8123;Compression=False;UseSession=False;Timeout=120;allowMultiQueries=true"
  }
}
```

#### 单元测试代码

```C#
using LiteSql;
using Models;
using Utils;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ClickHouseTest
{
    [TestClass]
    public class QueryTest
    {
        #region 测试查询数量
        [TestMethod]
        public void Test1Count()
        {
            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            long count = session.Queryable<PeopleFace>().Count();
            Console.WriteLine("总数=" + count.ToString("# #### #### ####"));
            Assert.IsTrue(count > 0);
        }
        #endregion

        #region 测试插入(ADO.NET原生)
        [TestMethod]
        public void Test2Insert1()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            using ClickHouseConnection conn = new ClickHouseConnection(connectionString);
            conn.Open();
            using ClickHouseCommand command = conn.CreateCommand();
            command.CommandText = @"insert into people_face_replica (captured_time, camera_id, camera_fun_type, face_id, data_source3, panoramic_image_url, portrait_image_url, event) 
                values ({captured_time:DateTime}, {camera_id:String}, {camera_fun_type:String}, {face_id:String}, {data_source3:String}, {panoramic_image_url:String}, {portrait_image_url:String}, {event:String})";

            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "captured_time", Value = new System.DateTime(2022, 1, 1) });
            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "camera_id", Value = "3401040578689" });
            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "camera_fun_type", Value = "2" });
            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "face_id", Value = "3401044900119031678978600000008888" });
            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "event", Value = "UPSERT" });
            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "panoramic_image_url", Value = "panoramic_image_url" });
            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "portrait_image_url", Value = "portrait_image_url" });
            command.Parameters.Add(new ClickHouseDbParameter() { ParameterName = "data_source3", Value = "" });
            command.ExecuteNonQuery();

            using ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);
            long count = session.Queryable<PeopleFace>().Where(t => t.CapturedTime >= new DateTime(2022, 1, 1)).Count();
            Console.WriteLine("count=" + count);
            Assert.IsTrue(count > 0);
        }
        #endregion

        #region 测试插入
        [TestMethod]
        public void Test2Insert2()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            PeopleFace peopleFace = new PeopleFace();
            peopleFace.CapturedTime = new DateTime(2022, 1, 1);
            peopleFace.CameraId = "34010400000000000000";
            peopleFace.FaceId = "340104490011905";
            peopleFace.CameraFunType = "2";
            peopleFace.PanoramicImageUrl = "PanoramicImageUrl";
            peopleFace.PortraitImageUrl = "PortraitImageUrl";
            peopleFace.Event = "UPSERT";
            session.Insert(peopleFace);

            long count = session.Queryable<PeopleFace>().Where(t => t.CapturedTime >= new DateTime(2022, 1, 1)).Count();
            Console.WriteLine("count=" + count);
            Assert.IsTrue(count > 0);
        }
        #endregion

        #region 测试批量插入
        [TestMethod]
        public void Test3BatchInsert()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            Random rnd = new Random();
            string pre = rnd.NextInt64(0, 10000000000).ToString();
            DateTime? time = null;
            List<PeopleFace> peopleFaceList = new List<PeopleFace>();
            for (int i = 0; i < 10; i++)
            {
                PeopleFace peopleFace = new PeopleFace();
                peopleFace.CapturedTime = DateTime.Now;
                peopleFace.CameraId = pre + "_" + i;
                peopleFace.FaceId = "340104490011903" + i;
                peopleFace.CameraFunType = "2";
                peopleFace.PanoramicImageUrl = "PanoramicImageUrl";
                peopleFace.PortraitImageUrl = "PortraitImageUrl";
                peopleFace.Event = "UPSERT";
                peopleFaceList.Add(peopleFace);

                if (time == null) time = peopleFace.CapturedTime;
            }
            session.Insert(peopleFaceList, 100); //设置合理的pageSize

            long count = session.Queryable<PeopleFace>().Where(t => t.CapturedTime >= time && t.CameraId.StartsWith(pre)).Count();
            Console.WriteLine("count=" + count);
            Assert.IsTrue(count > 0);
        }
        #endregion

        #region 测试修改
        [TestMethod]
        public void Test3Update()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            PeopleFace old = session.Queryable<PeopleFace>().Where(t => t.CameraId == "34010400000000000000").First();

            session.AttachOld(old);
            string newExtraInfo = DateTime.Now.ToString("yyyyMMddHHmmss");
            old.ExtraInfo = newExtraInfo;
            session.Update(old);

            Thread.Sleep(100);

            PeopleFace newPeopleFace = session.Queryable<PeopleFace>().Where(t => t.CameraId == "34010400000000000000").First();

            Console.WriteLine(newExtraInfo);
            Console.WriteLine(newPeopleFace.ExtraInfo);
            Assert.IsTrue(newPeopleFace.ExtraInfo == newExtraInfo);
        }
        #endregion

        #region 测试批量修改
        [TestMethod]
        public void Test4BatchUpdate()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            List<PeopleFace> oldList = session.Queryable<PeopleFace>().Where(t => t.CapturedTime > DateTime.Now.AddMinutes(-1)).QueryList<PeopleFace>();

            string newExtraInfo = DateTime.Now.ToString("yyyyMMddHHmmss");
            oldList.ForEach(old =>
            {
                session.AttachOld(old);
                old.ExtraInfo = newExtraInfo;
                session.Update(old);
            });
            //session.Update(oldList); //似乎不支持，错误信息：Multi-statements are not allowed

            Thread.Sleep(100);

            long count = session.Queryable<PeopleFace>().Where(t => t.ExtraInfo == newExtraInfo).Count();

            Console.WriteLine(count + "条已更新");
            Assert.IsTrue(count > 0);
        }
        #endregion

        #region 测试批量修改
        [TestMethod]
        public void Test4BatchUpdate2()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            string newExtraInfo = DateTime.Now.AddYears(-1).ToString("yyyyMMddHHmmss");

            //可以这样批量更新
            session.CreateSql<PeopleFace>("alter table people_face_replica update extra_info=@ExtraInfo where captured_time <= @Time", new { ExtraInfo = newExtraInfo, Time = DateTime.Now }).Execute();

            Thread.Sleep(100);

            long count = session.Queryable<PeopleFace>().Where(t => t.ExtraInfo == newExtraInfo).Count();

            Console.WriteLine(count + "条已更新");
            Assert.IsTrue(count > 0);
        }
        #endregion

        #region 删除
        [TestMethod]
        public void Test9Delete()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);
            long count = session.Queryable<PeopleFace>().Where(t => t.CapturedTime > DateTime.Now.AddMinutes(-1)).Count();
            Console.WriteLine("删除前数量=" + count);

            session.CreateSql("captured_time>@Time", new { Time = DateTime.Now.AddDays(-10) }).DeleteByCondition<PeopleFace>();

            Thread.Sleep(100);

            count = session.Queryable<PeopleFace>().Where(t => t.CapturedTime > DateTime.Now.AddMinutes(-1)).Count();
            Console.WriteLine("删除后数量=" + count);

            Assert.IsTrue(count == 0);
        }
        #endregion

        #region 测试参数化查询
        [TestMethod]
        public void Test5Query()
        {
            int queryCount = 10;
            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            List<PeopleFace> list = session.CreateSql("select * from people_face_replica t")
                .Append("where t.captured_time <= @EndTime", DateTime.Now)
                .Append("order by captured_time desc")
                .Append("limit " + queryCount)
                .QueryList<PeopleFace>();

            if (list.Count != queryCount)
            {
                Console.WriteLine(list.Count + " / " + queryCount);
            }
            else
            {
                Console.WriteLine("总数=" + list.Count);
            }
            Assert.IsTrue(list.Count == queryCount);

            list.ForEach(item => Console.WriteLine(ModelToStringUtil.ToString(item)));
        }
        #endregion

        #region 测试参数化查询(参数传匿名对象)
        [TestMethod]
        public void Test5Query2()
        {
            int queryCount = 10;
            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            List<PeopleFace> list = session.CreateSql("select * from people_face_replica t")
                .Append("where t.captured_time <= @EndTime", new { EndTime = DateTime.Now })
                .Append("order by captured_time desc")
                .AppendFormat("limit {0}", queryCount)
                .QueryList<PeopleFace>();

            if (list.Count != queryCount)
            {
                Console.WriteLine(list.Count + " / " + queryCount);
            }
            else
            {
                Console.WriteLine("总数=" + list.Count);
            }
            Assert.IsTrue(list.Count == queryCount);

            list.ForEach(item => Console.WriteLine(ModelToStringUtil.ToString(item)));
        }
        #endregion

        #region 测试参数化查询(Lambda表达式)
        [TestMethod]
        public void Test6QueryByLambda()
        {
            int queryCount = 10;
            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            List<PeopleFace> list = session.Queryable<PeopleFace>()
                .Where(t => t.CapturedTime <= DateTime.Now)
                .OrderByDescending(t => t.CapturedTime)
                .ToPageList(1, queryCount);

            if (list.Count != queryCount)
            {
                Console.WriteLine(list.Count + " / " + queryCount);
            }
            else
            {
                Console.WriteLine("总数=" + list.Count);
            }
            Assert.IsTrue(list.Count == queryCount);

            list.ForEach(item => Console.WriteLine(ModelToStringUtil.ToString(item)));
        }
        #endregion

        #region 测试参数化查询(Lambda表达式同名参数)
        [TestMethod]
        public void Test6QueryByLambda2()
        {
            ISession session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s);

            var sql = session.Queryable<PeopleFace>()
                .Where(t => t.CapturedTime <= DateTime.Now && t.CapturedTime >= new DateTime(2022, 1, 1));

            List<PeopleFace> list = session.Queryable<PeopleFace>()
                .Where(t => t.CapturedTime <= DateTime.Now && t.CapturedTime >= new DateTime(2022, 1, 1))
                .Append<PeopleFace>(" union all ", sql)
                .ToList();

            Console.WriteLine("总数=" + list.Count);
            list.ForEach(item => Console.WriteLine(ModelToStringUtil.ToString(item)));
            Assert.IsTrue(list.Count > 0);
        }
        #endregion

    }
}
```
