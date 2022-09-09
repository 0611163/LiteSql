using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using DAL;
using System.Collections.Generic;
using System.Linq;
using LiteSql;
using Utils;

namespace LiteSqlTest
{
    [TestClass]
    public class QueryTest
    {
        #region 变量
        private BsOrderDal m_BsOrderDal = ServiceHelper.Get<BsOrderDal>();
        #endregion

        #region 构造函数
        public QueryTest()
        {
            m_BsOrderDal.Preheat();
        }
        #endregion

        #region 测试查询订单集合
        [TestMethod]
        public void TestQuery()
        {
            List<BsOrder> list = m_BsOrderDal.GetList(0, "订单", DateTime.MinValue, DateTime.Now.AddDays(1), "100001,100002,100003");
            Assert.IsTrue(list.Count > 0);
            foreach (BsOrder item in list)
            {
                Console.WriteLine(ModelToStringUtil.ToString(item));
            }
        }
        #endregion

        #region 测试分页查询订单集合
        [TestMethod]
        public void TestQueryPage()
        {
            PageModel pageModel = new PageModel();
            pageModel.CurrentPage = 1;
            pageModel.PageSize = 10;

            List<BsOrder> list = m_BsOrderDal.GetListPage(ref pageModel, 0, null, DateTime.MinValue, DateTime.Now.AddDays(1));
            Assert.IsTrue(pageModel.TotalRows > 0);

            foreach (BsOrder item in list)
            {
                Console.WriteLine(ModelToStringUtil.ToString(item));
            }
            Console.WriteLine("totalRows=" + pageModel.TotalRows);
        }
        #endregion

        #region 测试AppendIf
        [TestMethod]
        public void TestAppendIf()
        {
            List<BsOrder> list = GetListForTestAppendIf(0, "订单", null, DateTime.Now.AddDays(1), "100001,100002,100003");

            foreach (BsOrder item in list)
            {
                Console.WriteLine(ModelToStringUtil.ToString(item));
            }
        }

        private List<BsOrder> GetListForTestAppendIf(int? status, string remark, DateTime? startTime, DateTime? endTime, string ids)
        {
            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                ISqlString sql = session.CreateSql(@"
                    select t.*, u.real_name as OrderUserRealName
                    from bs_order t
                    left join sys_user u on t.order_userid=u.id
                    where 1=1");

                sql.AppendIf(status.HasValue, " and t.status=@status", status);

                sql.AppendIf(!string.IsNullOrWhiteSpace(remark), " and t.remark like @remark", sql.ForContains(remark));

                sql.AppendIf(startTime.HasValue, " and t.order_time >= @startTime ", () => sql.ForDateTime(startTime.Value));

                sql.AppendIf(endTime.HasValue, " and t.order_time <= @endTime ", () => sql.ForDateTime(endTime.Value));

                sql.Append(" and t.id in @ids ", sql.ForList(ids.Split(',').ToList()));

                sql.Append(" order by t.order_time desc, t.id asc ");

                Assert.IsFalse(sql.SQL.Contains("and t.order_time >="));
                Assert.IsTrue(sql.SQL.Contains("and t.order_time <="));

                List<BsOrder> list = session.QueryList<BsOrder>(sql);
                return list;
            }
        }
        #endregion

        #region 测试查询订单集合(使用 ForContains、ForStartsWith、ForEndsWith、ForDateTime、ForList 等辅助方法)
        [TestMethod]
        public void TestQueryExt()
        {
            List<BsOrder> list = m_BsOrderDal.GetListExt(0, "订单", DateTime.MinValue, DateTime.Now.AddDays(1), "100001,100002,100003");

            Assert.IsTrue(list.Count > 0);
            foreach (BsOrder item in list)
            {
                Console.WriteLine(ModelToStringUtil.ToString(item));
            }
        }
        #endregion

        #region 测试Append方法传匿名对象
        [TestMethod]
        public void TestAppendWithAnonymous()
        {
            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                ISqlString sql = session.CreateSql(@"
                    select * from sys_user t where t.id <= @Id", new { Id = 20 });

                sql.Append(@" and t.create_userid = @userId 
                    and t.password = @password", new { userId = "1", password = "123456" });

                long id = session.CreateSql("select id from sys_user where id=@Id", new { Id = 1 }).QuerySingle<long>();
                Assert.IsTrue(id == 1);

                List<SysUser> list = session.QueryList<SysUser>(sql);
                foreach (SysUser item in list)
                {
                    Console.WriteLine(ModelToStringUtil.ToString(item));
                }
                Assert.IsTrue(list.Count > 0);
            }
        }
        #endregion

        #region 测试最佳实践
        [TestMethod]
        public void TestBestCode()
        {
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
            }
        }
        #endregion

        #region 测试最佳实践(原生SQL和Lambda表达式混写)
        [TestMethod]
        public void TestBestCode2()
        {
            DateTime? startTime = null;

            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                List<SysUser> list = session.Queryable<SysUser>()

                    .Append<SysUser>(@" where t.create_userid = @CreateUserId 
                        and t.password like @Password
                        and t.id in @Ids",
                        new
                        {
                            CreateUserId = "1",
                            Password = "%345%",
                            Ids = session.CreateSql().ForList(new List<int> { 1, 2, 9, 10, 11 })
                        })

                    .Where(t => !t.RealName.Contains("管理员"))

                    .Append<SysUser>(@" and t.create_time >= @StartTime", new { StartTime = new DateTime(2020, 1, 1) })

                    .Where<SysUser>(t => t.Id <= 20)

                    .AppendIf(startTime.HasValue, " and t.create_time >= @StartTime ", new { StartTime = startTime })

                    .Append(" and t.create_time <= @EndTime ", new { EndTime = new DateTime(2022, 8, 1) })

                    .QueryList<SysUser>();

                long id = session.Queryable<SysUser>().Where(t => t.Id == 1).First().Id;
                Assert.IsTrue(id == 1);

                foreach (SysUser item in list)
                {
                    Console.WriteLine(ModelToStringUtil.ToString(item));
                }
                Assert.IsTrue(list.Count > 0);
                Assert.IsTrue(list.Count(t => t.RealName.Contains("管理员")) == 0);
            }
        }
        #endregion

        #region 测试相同参数名称问题
        [TestMethod]
        public void TestSameParam()
        {
            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                var subSql = session.CreateSql<SysUser>("select t.Id from sys_user t")
                    .Where(t => t.RealName.Contains("李四") || t.RealName.Contains("王五"));

                var subSql2 = session.CreateSql<SysUser>("select t.Id from sys_user t").Where(t => t.Id <= 20);

                var sql = session.Queryable<SysUser>()

                    .Where(t => t.Password.Contains("345"))

                    .Append(" and id in ", subSql)

                    .Append<SysUser>(@" and t.create_time >= @StartTime", new { StartTime = new DateTime(2020, 1, 1) })

                    .Append<SysUser>(" and id in ", subSql2)

                    .Where(t => t.Password.Contains("234"));

                var sql2 = session.Queryable<SysUser>()
                    .Where(t => t.RealName.Contains("管理员") || t.RealName.Contains("张三"));

                sql.Append(" union all ", sql2);

                List<SysUser> list = sql.QueryList<SysUser>();

                foreach (SysUser item in list)
                {
                    Console.WriteLine(ModelToStringUtil.ToString(item));
                }
                Assert.IsTrue(list.Count > 0);
                Assert.IsTrue(list.Count(t => t.RealName.Contains("管理员")) > 0);
                Assert.IsTrue(list.Count(t => t.Id > 20) == 0);
            }
        }
        #endregion

        #region 测试子查询
        [TestMethod]
        public void TestSubQuery() //拼接子查询
        {
            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                var subSql = session.CreateSql<SysUser>().Select(t => new { t.Id }).Where(t => !t.RealName.Contains("管理员"));

                var subSql2 = session.CreateSql<SysUser>().Select(t => new { t.Id }).Where(t => t.Id <= 20);

                List<SysUser> list = session.Queryable<SysUser>()

                    .Where(t => t.Password.Contains("345"))

                    .Append(" and id in ", subSql)

                    .Append<SysUser>(@" and t.create_time >= @StartTime", new { StartTime = new DateTime(2020, 1, 1) })

                    .Append<SysUser>(" and id in ", subSql2)

                    .Where(t => t.Password.Contains("234"))

                    .QueryList<SysUser>();

                foreach (SysUser item in list)
                {
                    Console.WriteLine(ModelToStringUtil.ToString(item));
                }
                Assert.IsTrue(list.Count > 0);
                Assert.IsTrue(list.Count(t => t.RealName.Contains("管理员")) == 0);
                Assert.IsTrue(list.Count(t => t.Id > 20) == 0);
            }
        }
        #endregion

        #region 测试子查询
        [TestMethod]
        public void TestSubQuery2() //拼接子查询 union all
        {
            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                var subSql = session.CreateSql<SysUser>().Select(t => new { t.Id }).Where(t => !t.RealName.Contains("管理员"));

                var subSql2 = session.CreateSql<SysUser>().Select(t => new { t.Id }).Where(t => t.Id <= 20);

                var sql = session.Queryable<SysUser>()

                    .Where(t => t.Password.Contains("345"))

                    .Append(" and id in ", subSql)

                    .Append<SysUser>(@" and t.create_time >= @StartTime", new { StartTime = new DateTime(2020, 1, 1) })

                    .Append<SysUser>(" and id in ", subSql2)

                    .Where(t => t.Password.Contains("234"));

                var sql2 = session.Queryable<SysUser>().Where(t => t.RealName.Contains("管理员"));

                sql.Append(" union all ", sql2);

                List<SysUser> list = sql.QueryList<SysUser>();

                foreach (SysUser item in list)
                {
                    Console.WriteLine(ModelToStringUtil.ToString(item));
                }
                Assert.IsTrue(list.Count > 0);
                Assert.IsTrue(list.Count(t => t.RealName.Contains("管理员")) > 0);
                Assert.IsTrue(list.Count(t => t.Id > 20) == 0);
            }
        }
        #endregion

        #region 分组统计查询
        [TestMethod]
        public void TestGroupBy()
        {
            var session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

            List<BsOrder> list = session.Queryable<BsOrder>(o => new { o.Id, o.Remark, o.OrderTime })
                .Select("sum(d.price * d.quantity) as Amount")
                .LeftJoin<BsOrderDetail>((o, d) => o.Id == d.OrderId)
                .Append("group by  o.id, o.remark, o.order_time")
                .Append("order by Amount desc").QueryList<BsOrder>();

            foreach (BsOrder item in list)
            {
                Console.WriteLine(ModelToStringUtil.ToString(item));
            }
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void TestGroupBy2()
        {
            var session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

            List<BsOrder> list = session.CreateSql(@"
                select o.id, o.remark, o.order_time, sum(d.price * d.quantity) as amount
                from bs_order o
                left join bs_order_detail d on d.order_id = o.id
                group by o.id, o.remark, o.order_time
                order by amount desc").QueryList<BsOrder>();

            foreach (BsOrder item in list)
            {
                Console.WriteLine(ModelToStringUtil.ToString(item));
            }
            Assert.IsTrue(list.Count > 0);
        }
        #endregion

    }
}
