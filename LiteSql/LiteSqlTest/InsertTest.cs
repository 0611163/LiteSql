using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using DAL;
using System.Collections.Generic;
using LiteSql;
using Utils;
using System.Threading.Tasks;

namespace LiteSqlTest
{
    [TestClass]
    public class InsertTest
    {
        #region 变量
        private BsOrderDal m_BsOrderDal = ServiceHelper.Get<BsOrderDal>();
        private SysUserDal m_SysUserDal = ServiceHelper.Get<SysUserDal>();
        #endregion

        #region 构造函数
        public InsertTest()
        {
            m_BsOrderDal.Preheat();
        }
        #endregion

        #region 测试添加订单
        [TestMethod]
        public void TestInsertOrder()
        {
            string userId = "10";

            BsOrder order = new BsOrder();
            order.OrderTime = DateTime.Now;
            order.Amount = 0;
            order.OrderUserid = Convert.ToInt64(userId);
            order.Status = 0;
            order.CreateUserid = userId;

            List<BsOrderDetail> detailList = new List<BsOrderDetail>();
            BsOrderDetail detail = new BsOrderDetail();
            detail.GoodsName = "电脑";
            detail.Quantity = 3;
            detail.Price = 5100;
            detail.Spec = "台";
            detail.CreateUserid = userId;
            detail.OrderNum = 1;
            detailList.Add(detail);

            detail = new BsOrderDetail();
            detail.GoodsName = "鼠标";
            detail.Quantity = 12;
            detail.Price = (decimal)50.68;
            detail.Spec = "个";
            detail.CreateUserid = userId;
            detail.OrderNum = 2;
            detailList.Add(detail);

            detail = new BsOrderDetail();
            detail.GoodsName = "键盘";
            detail.Quantity = 11;
            detail.Price = (decimal)123.66;
            detail.Spec = "个";
            detail.CreateUserid = userId;
            detail.OrderNum = 3;
            detailList.Add(detail);

            m_BsOrderDal.Insert(order, detailList);
        }
        #endregion

        #region 测试添加两个订单
        /// <summary>
        /// 测试新增两条订单记录
        /// </summary>
        [TestMethod]
        public void TestInsertTwoOrder()
        {
            TestInsertOrder();
            TestInsertOrder();
        }
        #endregion

        #region 测试添加用户
        [TestMethod]
        public void TestInsertUser()
        {
            SysUser user = new SysUser();
            user.UserName = "testUser";
            user.RealName = "测试插入用户";
            user.Password = "123456";
            user.CreateUserid = "1";

            user.Id = m_SysUserDal.Insert(user);
            Console.WriteLine("user.Id=" + user.Id);
        }
        #endregion

        #region 测试添加用户(异步)
        [TestMethod]
        public async Task TestInsertUserAsync()
        {
            SysUser user = new SysUser();
            user.UserName = "testUser";
            user.RealName = "测试插入用户";
            user.Password = "123456";
            user.CreateUserid = "1";

            var task = m_SysUserDal.InsertAsync(user);
            await task;
        }
        #endregion

    }
}
