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
            SysUser oldUser = null;
            using (var session = LiteSqlFactory.GetSession())
            {
                oldUser = session.Query<SysUser>("select * from sys_user");
            }

            using (var session = LiteSqlFactory.GetSession())
            {
                session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

                session.AttachOld(oldUser);
                SysUser user = session.Query<SysUser>("select * from sys_user");
                user.Username = "testUser";
                user.Realname = "测试修改用户3";
                user.Password = "123456";
                user.Updateuserid = "1";
                user.Updatetime = DateTime.Now;
                session.Update(user);

                SqlString sql = session.CreateSqlString("select * from sys_user where \"RealName\" like @RealName", new { RealName = "测试修改用户%" });
                long count = session.QueryCount(sql.SQL, sql.Params);
                Assert.IsTrue(count > 0);
            }
        }
    }
}
