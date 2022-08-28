﻿using System;
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

                SqlString sql = session.CreateSql(@"
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

                SqlString sql = session.CreateSql(@"
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
        }
        #endregion

    }
}