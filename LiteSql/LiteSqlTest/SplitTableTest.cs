using DAL;
using LiteSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace LiteSqlTest
{
    /// <summary>
    /// 手动分表测试
    /// </summary>
    [TestClass]
    public class SplitTableTest
    {
        #region 变量
        private BsOrderDal m_BsOrderDal = ServiceHelper.Get<BsOrderDal>();
        private Random _rnd = new Random();
        #endregion

        #region 构造函数
        public SplitTableTest()
        {
            m_BsOrderDal.Preheat();
        }
        #endregion

        #region 插入测试
        [TestMethod]
        public void Test1Insert()
        {
            SysUser user = new SysUser();
            user.UserName = "testUser";
            user.RealName = "测试插入分表数据";
            user.Remark = "测试插入分表数据";
            user.Password = "123456";
            user.CreateUserid = "1";
            user.CreateTime = DateTime.Now;

            SplitTableMapping splitTableMapping = new SplitTableMapping(typeof(SysUser), "sys_user_202208");

            using (var session = LiteSqlFactory.GetSession(splitTableMapping))
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                session.Insert(user);

                user.Id = session.QuerySingle<long>("select @@IDENTITY");
                Console.WriteLine("插入成功, user.Id=" + user.Id);
            }

        }
        #endregion

        #region 修改测试
        [TestMethod]
        public void Test2Update()
        {
            long userId = 10;
            SysUser user = null;

            SplitTableMapping splitTableMapping = new SplitTableMapping(typeof(SysUser), "sys_user_202208");

            using (var session = LiteSqlFactory.GetSession(splitTableMapping))
            {
                user = session.QueryById<SysUser>(userId);
            }

            if (user != null)
            {
                using (var session = LiteSqlFactory.GetSession(splitTableMapping))
                {
                    session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                    session.AttachOld(user);

                    user.UpdateUserid = "1";
                    user.Remark = "测试修改分表数据" + _rnd.Next(1, 100);
                    user.UpdateTime = DateTime.Now;

                    session.Update(user);
                }
                Console.WriteLine("用户 ID=" + user.Id + " 已修改");
            }
            else
            {
                throw new Exception("测试数据被删除");
            }
        }
        #endregion

        #region 删除测试
        [TestMethod]
        public void Test3Delete()
        {
            SplitTableMapping splitTableMapping = new SplitTableMapping(typeof(SysUser), "sys_user_202208");
            using (var session = LiteSqlFactory.GetSession(splitTableMapping))
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                int deleteCount = session.DeleteByCondition<SysUser>(string.Format("id>20"));
                Console.WriteLine(deleteCount + "条数据已删除");
                int deleteCount2 = session.DeleteById<SysUser>(10000);
                Console.WriteLine(deleteCount2 + "条数据已删除");
            }
        }
        #endregion

        #region 查询测试
        [TestMethod]
        public void Test4Query()
        {
            SplitTableMapping splitTableMapping = new SplitTableMapping(typeof(SysUser), "sys_user_202208");

            List<SysUser> list = new List<SysUser>();
            using (var session = LiteSqlFactory.GetSession(splitTableMapping))
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                SqlString<SysUser> sql = session.CreateSqlString<SysUser>();

                list = sql.Select()
                    .Where(t => t.Id < 10)
                    .OrderBy(t => t.Id)
                    .ToList();
            }

            foreach (SysUser item in list)
            {
                Console.WriteLine(ModelToStringUtil.ToString(item));
            }
        }
        #endregion

    }
}
