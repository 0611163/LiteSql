using DAL;
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
    [TestClass]
    public class BatchUpdateTest
    {
        #region 变量
        private BsOrderDal m_BsOrderDal = ServiceHelper.Get<BsOrderDal>();
        private SysUserDal m_SysUserDal = ServiceHelper.Get<SysUserDal>();
        private Random _rnd = new Random();
        #endregion

        #region 构造函数
        public BatchUpdateTest()
        {
            m_BsOrderDal.Preheat();
        }
        #endregion

        #region 测试批量修改用户
        [TestMethod]
        public void TestUpdateUserList()
        {
            List<SysUser> userList = m_SysUserDal.GetList("select t.* from sys_user t where t.id > 20");

            foreach (SysUser user in userList)
            {
                user.Remark = "测试修改用户" + _rnd.Next(1, 10000);
                user.UpdateUserid = "1";
                user.UpdateTime = DateTime.Now;
            }

            Console.WriteLine("开始 count=" + userList.Count);
            DateTime dt = DateTime.Now;

            m_SysUserDal.Update(userList);

            string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
            Console.WriteLine("结束，耗时：" + time + "秒");
        }
        #endregion

        #region 测试批量修改用户(异步)
        [TestMethod]
        public async Task TestUpdateUserListAsync()
        {
            List<SysUser> userList = m_SysUserDal.GetList("select t.* from sys_user t where t.id > 20");

            foreach (SysUser user in userList)
            {
                user.Remark = "测试修改用户" + _rnd.Next(1, 10000);
                user.UpdateUserid = "1";
                user.UpdateTime = DateTime.Now;
            }

            Console.WriteLine("开始 count=" + userList.Count);
            DateTime dt = DateTime.Now;

            await m_SysUserDal.UpdateAsync(userList);

            string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
            Console.WriteLine("结束，耗时：" + time + "秒");
        }
        #endregion

    }
}
