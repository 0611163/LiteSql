using LiteSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using System;
using System.Collections.Generic;
using System.Threading;
using Utils;

namespace OracleTest
{
    [TestClass]
    public class QueryTest
    {
        #region 构造函数
        public QueryTest()
        {
            ThreadPool.SetMaxThreads(1000, 1000);
            ThreadPool.SetMinThreads(200, 200);

            //预热
            using (var session = LiteSqlFactoryMySQL.GetSession())
            {
                session.QuerySingle("select count(*) from bs_order");
            }
            using (var session = LiteSqlFactory.GetSession())
            {
                session.QuerySingle("select count(*) from CARINFO_MERGE");
            }
        }
        #endregion

        [TestMethod]
        public void Test1Query()
        {
            List<CarinfoMerge> list = new List<CarinfoMerge>();

            using (var session = LiteSqlFactory.GetSession())
            {
                string sql = "select * from CARINFO_MERGE where rownum<20000";
                list = session.QueryList<CarinfoMerge>(sql);
            }

            Assert.IsTrue(list.Count > 0);

            int outputCount = 0;
            foreach (CarinfoMerge item in list)
            {
                if (outputCount++ < 20)
                {
                    Console.WriteLine(ModelToStringUtil.ToString(item));
                }
            }
        }

        [TestMethod]
        public void Test2QueryMySqlAndOracle()
        {
            using (var session = LiteSqlFactoryMySQL.GetSession())
            {
                BsOrder order = session.QueryById<BsOrder>("100001");
                Assert.IsTrue(order != null);
                Console.WriteLine("订单：");
                Console.WriteLine(ModelToStringUtil.ToString(order));
            }

            using (var session = LiteSqlFactory.GetSession())
            {
                SqlString sql = session.CreateSqlString("select * from CARINFO_MERGE where rownum<1000");

                //sql.Append(" and id in @ids", sql.ForList(new List<long> { 715299 }));

                LogTimeUtil logTime = new LogTimeUtil();
                List<CarinfoMerge> list = session.QueryList<CarinfoMerge>(sql.SQL, sql.Params);
                Assert.IsTrue(list.Count > 0);
                Console.WriteLine("CARINFO_MERGE：");
                for (int i = 0; i < 20; i++)
                {
                    Console.WriteLine(ModelToStringUtil.ToString(list[i]));
                }
            }
        }
    }
}
