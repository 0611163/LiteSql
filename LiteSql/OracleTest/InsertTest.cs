using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using LiteSql;
using Utils;

namespace OracleTest
{
    [TestClass]
    public class InsertTest
    {
        #region 变量
        #endregion

        #region 构造函数
        public InsertTest()
        {
            //预热
            using (var session = LiteSqlFactory.GetSession())
            {
                session.QuerySingle("select count(*) from CARINFO_MERGE");
            }
        }
        #endregion

        [TestMethod]
        public void TestInsertUpdate()
        {
            CarinfoMerge info = new CarinfoMerge();
            info.LicenseNo = "皖A00000";
            info.ModifyTime = DateTime.Now;
            info.CarPlateColor = "蓝";
            info.CarColor = "白";
            info.Brand = "普通货车";
            info.TotalMass = (decimal)100.55;
            info.BeginTime = new DateTime(2020, 1, 1);
            info.High = 8;

            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                info.Id = session.QueryNextId<CarinfoMerge>();
                session.Insert(info);

                ISqlString sql = session.CreateSql("select * from CARINFO_MERGE where id=@id", info.Id);

                CarinfoMerge carinfo = sql.Query<CarinfoMerge>();
                Assert.AreEqual(carinfo.High, 8);
                Assert.AreEqual(carinfo.BeginTime, new DateTime(2020, 1, 1));
                Console.WriteLine(ModelToStringUtil.ToString(carinfo));
            }

            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                info = session.QueryById<CarinfoMerge>(info.Id);
                session.AttachOld(info); //使只更新有变化的字段
                info.ModifyTime = DateTime.Now;
                info.High = 9;
                info.BeginTime = new DateTime(2020, 1, 2);
                info.TotalMass = (decimal)100.66;
                session.Update(info);

                ISqlString sql = session.CreateSql("select * from CARINFO_MERGE where id=@id", info.Id);

                CarinfoMerge carinfo = sql.Query<CarinfoMerge>();
                Assert.AreEqual(carinfo.High, 9);
                Assert.AreEqual(carinfo.BeginTime, new DateTime(2020, 1, 2));
                Console.WriteLine(ModelToStringUtil.ToString(carinfo));
            }
        }

    }
}
