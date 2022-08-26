using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using LiteSql;

namespace PostgreSQLTest
{
    public partial class PostgreSQLTest
    {
        [TestMethod]
        public void Test3Update()
        {
            try
            {
                using (var session = LiteSqlFactory.GetSession())
                {
                    session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                    SysUser user = session.Query<SysUser>("select * from sys_user");
                    session.AttachOld(user);
                    user.Username = "testUser";
                    user.Realname = "测试插入用户";
                    user.Password = "123456";
                    user.Updateuserid = "1";
                    user.Updatetime = DateTime.Now;
                    session.Update(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}
