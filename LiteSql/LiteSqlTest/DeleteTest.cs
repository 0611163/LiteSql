using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using DAL;
using System.Collections.Generic;
using LiteSql;
using Utils;

namespace LiteSqlTest
{
    [TestClass]
    public class DeleteTest
    {
        #region 变量
        private BsOrderDal m_BsOrderDal = ServiceHelper.Get<BsOrderDal>();
        private SysUserDal m_SysUserDal = ServiceHelper.Get<SysUserDal>();
        #endregion

        #region 构造函数
        public DeleteTest()
        {
            m_BsOrderDal.Preheat();
        }
        #endregion

        #region 测试删除用户
        [TestMethod]
        public void TestDeleteUser()
        {
            SysUser user = new SysUser();
            user.UserName = "testUser";
            user.RealName = "测试插入用户";
            user.Password = "123456";
            user.CreateUserid = "1";
            long id = m_SysUserDal.Insert(user);

            m_SysUserDal.Delete(id);
        }
        #endregion

        #region 测试根据查询条件删除用户
        [TestMethod]
        public void TestDeleteUserByCondition()
        {
            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) =>
                {
                    Console.WriteLine(s); //打印SQL
                };

                session.DeleteByCondition<SysUser>(string.Format("id>20"));
            }
        }
        #endregion

        #region 删除订单和订单明细
        [TestMethod]
        public void TestDeleteOrderByCondition()
        {
            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (sql, param) => Console.WriteLine(sql); //打印SQL

                SqlString deleteSql = session.CreateSqlString("id not like @Id", new { Id = "10000_" });
                int rows = session.DeleteByCondition<BsOrder>(deleteSql.SQL, deleteSql.Params);
                Console.WriteLine("BsOrder表" + rows + "行已删除");
                rows = session.DeleteByCondition<BsOrderDetail>(string.Format("order_id not like '10000_'"));
                Console.WriteLine("BsOrderDetail表" + rows + "行已删除");
            }
        }
        #endregion

    }
}
