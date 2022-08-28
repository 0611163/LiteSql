# LiteSql

## 简介

一款使用原生SQL查询的轻量级ORM，支持Oracle、MSSQL、MySQL、PostgreSQL、SQLite、Access数据库。

## 经典示例

```C#
DateTime? startTime = null;

using (var session = LiteSqlFactory.GetSession())
{
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
                Ids = session.CreateSql().ForList(new List<int> { 1, 2, 9, 10, 11 })
            })

        .AppendIf(startTime.HasValue, " and t.create_time < @StartTime ", () => new { StartTime = startTime.Value })

        .Append(" and t.create_time < @CreateTime ", new { CreateTime = new DateTime(2022, 8, 1) })

        .QueryList<SysUser>();

    long id = session.CreateSql("select id from sys_user where id=@Id", new { Id = 1 })
        .QuerySingle<long>();
    Assert.IsTrue(id == 1);

    foreach (SysUser item in list)
    {
        Console.WriteLine(ModelToStringUtil.ToString(item));
    }
    Assert.IsTrue(list.Count > 0);
}
```

## 特点

1. 支持Oracle、MSSQL、MySQL、PostgreSQL、SQLite五种数据库
2. 可以很方便地支持任意关系数据库
3. 有配套的Model生成器
4. insert、update、delete操作无需写SQL
5. 查询使用原生SQL
6. 查询结果通过映射转成实体类或实体类集合
7. 支持参数化查询，通过SqlString类提供非常方便的参数化查询
8. 支持连接多个数据源
9. 支持手动分表
10. 单表查询、单表分页查询、简单的联表分页查询支持Lambda表达式
11. 支持原生SQL和Lambda表达式混写

## 优点

1. 比较简单，学习成本低
2. 查询使用原生SQL

## 缺点

1. 对Lambda表达式的支持比较弱
2. 复杂查询不支持Lambda表达式(子查询、分组统计查询、嵌套查询等不支持)

## 建议

1. 单表查询、简单的连表查询可以使用Lambda表达式
2. 复杂查询建议使用原生SQL
3. 如果出现不支持的Lambda表达式写法，请使用原生SQL替代

## 开发环境

1. VS2022
2. 测试工程使用.NET Framework 4.5.2

## 配套Model生成器地址：

[https://gitee.com/s0611163/ModelGenerator](https://gitee.com/s0611163/ModelGenerator)

## Dapper版

使用ADO.NET操作数据库改成了使用Dapper操作数据库

[https://gitee.com/s0611163/Dapper.LiteSql/](https://gitee.com/s0611163/Dapper.LiteSql/)

## 作者邮箱

    651029594@qq.com

## 使用步骤

1. 安装LiteSql

```text
Install-Package LiteSql -Version 1.5.2
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
        private static ILiteSqlClient _liteSqlClient = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.MySQL);
        #endregion

        #region 静态构造函数
        static LiteSqlFactory()
        {
            ProviderFactory.RegisterDBProvider(DBType.MySQL, new MySQLProvider());
        }
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        public static ISession GetSession()
        {
            return _liteSqlClient.GetSession();
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        public static async Task<ISession> GetSessionAsync()
        {
            return await _liteSqlClient.GetSessionAsync();
        }
        #endregion

    }
}
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
    [Column("id")]
    public string Id { get; set; }

    /// <summary>
    /// 订单时间
    /// </summary>
    [Column("order_time")]
    public DateTime OrderTime { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    [Column("amount")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// 下单用户
    /// </summary>
    [Column("order_userid")]
    public long OrderUserid { get; set; }

    /// <summary>
    /// 订单状态(0草稿 1已下单 2已付款 3已发货 4完成)
    /// </summary>
    [Column("status")]
    public int Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [Column("remark")]
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
    public List<BsOrderDetail> DetailList { get; set; }

    /// <summary>
    /// 下单用户姓名
    /// </summary>
    public string OrderUserRealName { get; set; }

    /// <summary>
    /// 下单用户名
    /// </summary>
    public string OrderUserName { get; set; }
}
```

## 增删改查示例

### 添加

```C#
public void Insert(SysUser info)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        session.Insert(info);
    }
}
```

### 批量添加

```C#
public void Insert(List<SysUser> list)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        session.Insert(list);
    }
}
```

### 修改

```C#
public void Update(SysUser info)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        session.Update(info);
    }
}
```

### 批量修改

```C#
public void Update(List<SysUser> list)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        session.Update(list);
    }
}
```

### 修改时只更新数据有变化的字段

```C#
using (var session = LiteSqlFactory.GetSession())
{
    session.AttachOld(user); //附加更新前的旧数据，只更新数据发生变化的字段，提升更新性能

    user.UpdateUserid = "1";
    user.Remark = "测试修改用户" + _rnd.Next(1, 100);
    user.UpdateTime = DateTime.Now;

    session.Update(user);
}
```

```C#
using (var session = LiteSqlFactory.GetSession())
{
    session.AttachOld(userList); //附加更新前的旧数据，只更新数据发生变化的字段，提升更新性能

    foreach (SysUser user in userList)
    {
        user.Remark = "测试修改用户" + _rnd.Next(1, 10000);
        user.UpdateUserid = "1";
        user.UpdateTime = DateTime.Now;
    }

    session.Update(userList);
}
```

### 删除


```C#
public void Delete(string id)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        session.DeleteById<SysUser>(id);
    }
}
```

### 条件删除

```C#
using (var session = LiteSqlFactory.GetSession())
{
    session.DeleteByCondition<SysUser>(string.Format("id>=12"));
}
```

### 查询单个记录

```C#
public SysUser Get(string id)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        return session.QueryById<SysUser>(id);
    }
}
```

```C#
using (var session = LiteSqlFactory.GetSession())
{
    return session.Query<SysUser>("select * from sys_user");
}
```

### 简单查询

```C#
using (var session = LiteSqlFactory.GetSession())
{
    string sql = "select * from CARINFO_MERGE";
    List<CarinfoMerge> result = session.QueryList<CarinfoMerge>(sql);
}
```

### 条件查询

```C#
public List<BsOrder> GetList(int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        SqlString sql = session.CreateSqlString(@"
            select t.*, u.real_name as OrderUserRealName
            from bs_order t
            left join sys_user u on t.order_userid=u.id
            where 1=1");

        sql.AppendIf(status.HasValue, " and t.status=@status", status);

        sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like concat('%',@remark,'%')", remark);

        sql.AppendIf(startTime.HasValue, " and t.order_time>=STR_TO_DATE(@startTime, '%Y-%m-%d %H:%i:%s') ", () => startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));

        sql.AppendIf(endTime.HasValue, " and t.order_time<=STR_TO_DATE(@endTime, '%Y-%m-%d %H:%i:%s') ", () => endTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));

        sql.Append(" order by t.order_time desc, t.id asc ");

        List<BsOrder> list = session.QueryList<BsOrder>(sql);
        return list;
    }
}
```

### 条件查询(SQL参数支持匿名对象)

```C#
public List<BsOrder> GetList(int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        SqlString sql = session.CreateSqlString(@"
            select t.*, u.real_name as OrderUserRealName
            from bs_order t
            left join sys_user u on t.order_userid=u.id
            where 1=1");

        sql.AppendIf(status.HasValue, " and t.status=@status", new { status = status });

        sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like concat('%',@remark,'%')", new { remark = remark });

        sql.AppendIf(startTime.HasValue, " and t.order_time>=STR_TO_DATE(@startTime, '%Y-%m-%d %H:%i:%s') ", () => new { startTime = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss") });

        sql.AppendIf(endTime.HasValue, " and t.order_time<=STR_TO_DATE(@endTime, '%Y-%m-%d %H:%i:%s') ", () => new { endTime = endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") });

        sql.Append(" order by t.order_time desc, t.id asc ");

        List<BsOrder> list = session.QueryList<BsOrder>(sql);
        return list;
    }
}
```

### 分页查询

```C#
public List<BsOrder> GetListPage(ref PageModel pageModel, int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        SqlString sql = session.CreateSqlString(@"
            select t.*, u.real_name as OrderUserRealName
            from bs_order t
            left join sys_user u on t.order_userid=u.id
            where 1=1");

        sql.AppendIf(status.HasValue, " and t.status=@status", status);

        sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like concat('%',@remark,'%')", remark);

        sql.AppendIf(startTime.HasValue, " and t.order_time>=STR_TO_DATE(@startTime, '%Y-%m-%d %H:%i:%s') ", () => startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));

        sql.AppendIf(endTime.HasValue, " and t.order_time<=STR_TO_DATE(@endTime, '%Y-%m-%d %H:%i:%s') ", () => endTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));

        string orderby = " order by t.order_time desc, t.id asc ";
        
        pageModel.TotalRows = session.QueryCount(sql);
        return session.QueryPage<BsOrder>(sql, orderby, pageModel.PageSize, pageModel.CurrentPage);
    }
}
```


### 事务

```C#
public string Insert(BsOrder order, List<BsOrderDetail> detailList)
{
    using (var session = LiteSqlFactory.GetSession())
    {
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
}
```

### 异步查询

```C#
public async Task<List<BsOrder>> GetListPageAsync(PageModel pageModel, int? status, string remark, DateTime? startTime, DateTime? endTime)
{
    using (var session = await LiteSqlFactory.GetSessionAsync())
    {
        SqlString sql = session.CreateSqlString(@"
            select t.*, u.real_name as OrderUserRealName
            from bs_order t
            left join sys_user u on t.order_userid=u.id
            where 1=1");

        sql.AppendIf(status.HasValue, " and t.status=@status", status);

        sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like concat('%',@remark,'%')", remark);

        sql.AppendIf(startTime.HasValue, " and t.order_time>=STR_TO_DATE(@startTime, '%Y-%m-%d %H:%i:%s') ", () => startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));

        sql.AppendIf(endTime.HasValue, " and t.order_time<=STR_TO_DATE(@endTime, '%Y-%m-%d %H:%i:%s') ", () => endTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));

        string orderby = " order by t.order_time desc, t.id asc ";
        
        var countResult = await session.QueryCountAsync(sql, pageModel.PageSize);
        pageModel.TotalRows = countResult.Count;
        return await session.QueryPageAsync<BsOrder>(sql, orderby, pageModel.PageSize, pageModel.CurrentPage);
    }
}
```

### 条件查询(使用 ForContains、ForStartsWith、ForEndsWith、ForDateTime、ForList 等辅助方法)

```C#
public List<BsOrder> GetListExt(int? status, string remark, DateTime? startTime, DateTime? endTime, string ids)
{
    using (var session = LiteSqlFactory.GetSession())
    {
        SqlString sql = session.CreateSqlString(@"
            select t.*, u.real_name as OrderUserRealName
            from bs_order t
            left join sys_user u on t.order_userid=u.id
            where 1=1");

        sql.AppendIf(status.HasValue, " and t.status=@status", status);

        sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like @remark", sql.ForContains(remark));

        sql.AppendIf(startTime.HasValue, " and t.order_time >= @startTime ", sql.ForDateTime(startTime.Value));

        sql.AppendIf(endTime.HasValue, " and t.order_time <= @endTime ", sql.ForDateTime(endTime.Value));

        sql.Append(" and t.id in @ids ", sql.ForList(ids.Split(',').ToList()));

        sql.Append(" order by t.order_time desc, t.id asc ");

        List<BsOrder> list = session.QueryList<BsOrder>(sql);
        return list;
    }
}
```

### 使用Lambda表达式单表查询

单表分页查询使用ToPageList替换ToList即可

```C#
public void TestQueryByLambda6()
{
    using (var session = LiteSqlFactory.GetSession())
    {
        SqlString<BsOrder> sql = session.CreateSqlString<BsOrder>();

        string remark = "测试";

        List<BsOrder> list = sql.Select()

            .WhereIf(!string.IsNullOrWhiteSpace(remark),
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
}
```

### 使用Lambda表达式联表分页查询(简单的联表查询，复杂情况请使用原生SQL或原生SQL和Lambda表达式混写)

```C#
public void TestQueryByLambda7()
{
    using (var session = LiteSqlFactory.GetSession())
    {
        SqlString<BsOrder> sql = session.CreateSqlString<BsOrder>();

        int total;
        List<string> idsNotIn = new List<string>() { "100007", "100008", "100009" };

        List<BsOrder> list = sql.Select()
            .Select<SysUser>(u => u.UserName, t => t.OrderUserName)
            .Select<SysUser>(u => u.RealName, t => t.OrderUserRealName)
            .LeftJoin<SysUser>((t, u) => t.OrderUserid == u.Id)
            .LeftJoin<BsOrderDetail>((t, d) => t.Id == d.OrderId)
            .Where<SysUser, BsOrderDetail>((t, u, d) => t.Remark.Contains("订单") && u.CreateUserid == "1" && d.GoodsName != null)
            .WhereIf<BsOrder>(true, t => t.Remark.Contains("测试"))
            .WhereIf<BsOrder>(true, t => !idsNotIn.Contains(t.Id))
            .WhereIf<SysUser>(true, u => u.CreateUserid == "1")
            .OrderByDescending(t => t.OrderTime).OrderBy(t => t.Id)
            .ToPageList(1, 20, out total);

        foreach (BsOrder item in list)
        {
            Console.WriteLine(ModelToStringUtil.ToString(item));
        }
    }
}
```

### 原生SQL和Lambda表达式混写

```C#
public void TestQueryByLambda9()
{
    using (var session = LiteSqlFactory.GetSession())
    {
        SqlString<BsOrder> sql = session.CreateSqlString<BsOrder>(@"
            select t.*, u.real_name as OrderUserRealName
            from bs_order t
            left join sys_user u on t.order_userid=u.id
            where 1=1");

        List<BsOrder> list = sql.Where(t => t.Status == int.Parse("0")
            && t.Status == new BsOrder().Status
            && t.Remark.Contains("订单")
            && t.Remark != null
            && t.OrderTime >= new DateTime(2010, 1, 1)
            && t.OrderTime <= DateTime.Now.AddDays(1))
            .WhereIf<SysUser>(true, u => u.CreateTime < DateTime.Now)
            .OrderByDescending(t => t.OrderTime).OrderBy(t => t.Id)
            .ToList();

        foreach (BsOrder item in list)
        {
            Console.WriteLine(ModelToStringUtil.ToString(item));
        }
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
        private static ILiteSqlClient _liteSqlClient = new LiteSqlClient(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), DBType.MySQL);
        #endregion

        #region 静态构造函数
        static LiteSqlFactory()
        {
            ProviderFactory.RegisterDBProvider(DBType.MySQL, new MySQLProvider());
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

### 数据插入

```C#
SplitTableMapping splitTableMapping = new SplitTableMapping(typeof(SysUser), "sys_user_202208");

using (var session = LiteSqlFactory.GetSession(splitTableMapping))
{
    session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

    session.Insert(user);

    user.Id = session.QuerySingle<long>("select @@IDENTITY");
    Console.WriteLine("插入成功, user.Id=" + user.Id);
}
```

### 数据更新

```C#
SplitTableMapping splitTableMapping = new SplitTableMapping(typeof(SysUser), "sys_user_202208");

using (var session = LiteSqlFactory.GetSession(splitTableMapping))
{
    session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

    session.AttachOld(user); //附加更新前的旧数据，只更新数据发生变化的字段，提升更新性能

    user.UpdateUserid = "1";
    user.Remark = "测试修改分表数据" + _rnd.Next(1, 100);
    user.UpdateTime = DateTime.Now;

    session.Update(user);
}
```

### 数据删除

```C#
SplitTableMapping splitTableMapping = new SplitTableMapping(typeof(SysUser), "sys_user_202208");
using (var session = LiteSqlFactory.GetSession(splitTableMapping))
{
    session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

    int deleteCount = session.DeleteByCondition<SysUser>(string.Format("id>20"));
    Console.WriteLine(deleteCount + "条数据已删除");
    int deleteCount2 = session.DeleteById<SysUser>(10000);
    Console.WriteLine(deleteCount2 + "条数据已删除");
}
```

### 数据查询

```C#
using (var session = LiteSqlFactory.GetSession(splitTableMapping))
{
    session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

    SqlString<SysUser> sql = session.CreateSqlString<SysUser>();

    List<SysUser> list = sql.Select()
        .Where(t => t.Id < 10)
        .OrderBy(t => t.Id)
        .ToList();
}
```

## 支持更多数据库

    现有架构实际上支持任何传统关系型数据库

### 如何实现

    以PostgreSQL为例，假如该库尚未支持PostgreSQL

1. 定义一个数据库提供者类，实现IProvider接口

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

        #region GetParameterMark
        public string GetParameterMark()
        {
            return "@";
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

        #region ForContains
        public SqlValue ForContains(string value)
        {
            return new SqlValue("concat('%',{0},'%')", value);
        }
        #endregion

        #region ForStartsWith
        public SqlValue ForStartsWith(string value)
        {
            return new SqlValue("concat({0},'%')", value);
        }
        #endregion

        #region ForEndsWith
        public SqlValue ForEndsWith(string value)
        {
            return new SqlValue("concat('%',{0})", value);
        }
        #endregion

        #region ForDateTime
        public SqlValue ForDateTime(DateTime dateTime)
        {
            return new SqlValue("TO_TIMESTAMP(CAST({0} as TEXT), 'yyyy-MM-dd hh24:mi:ss')", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
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

如果觉得需要实现的接口太多太麻烦，可以写个不支持lambda表达式的版本，即不实现For开头的接口，如下所示：

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

        #region GetParameterMark
        public string GetParameterMark()
        {
            return "@";
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

        public SqlValue ForContains(string value)
        {
            throw new NotImplementedException();
        }

        public SqlValue ForStartsWith(string value)
        {
            throw new NotImplementedException();
        }

        public SqlValue ForEndsWith(string value)
        {
            throw new NotImplementedException();
        }

        public SqlValue ForDateTime(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public SqlValue ForList(IList list)
        {
            throw new NotImplementedException();
        }

    }
}
```

2. 定义LiteSqlFactory类

    先注册数据库提供者

```C#
ProviderFactory.RegisterDBProvider(typeof(PostgreSQLProvider), new PostgreSQLProvider());
```

    再创建LiteSqlClient对象

```C#
new LiteSqlClient(connectionString, typeof(PostgreSQLProvider))
```

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
            ProviderFactory.RegisterDBProvider(typeof(PostgreSQLProvider), new PostgreSQLProvider());

            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("config.json");
            var configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            _liteSqlClient = new LiteSqlClient(connectionString, typeof(PostgreSQLProvider));
        }
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        public static ISession GetSession()
        {
            return _liteSqlClient.GetSession();
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        public static async Task<ISession> GetSessionAsync()
        {
            return await _liteSqlClient.GetSessionAsync();
        }
        #endregion

    }
}
```

    然后就可以使用了
