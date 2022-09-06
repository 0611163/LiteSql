using DAL;
using LiteSql;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PerformanceTest
{
    public partial class Form1 : Form
    {
        #region 变量
        private BsOrderDal m_BsOrderDal = ServiceHelper.Get<BsOrderDal>();
        private SysUserDal m_SysUserDal = ServiceHelper.Get<SysUserDal>();
        private Random _rnd = new Random();
        private int _count = 10000;
        #endregion

        #region Form1
        public Form1()
        {
            InitializeComponent();
        }
        #endregion

        #region Form1_Load
        private void Form1_Load(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                m_BsOrderDal.Preheat(); //预热
                Log("预热完成");
            });
        }
        #endregion

        #region Log
        private void Log(string log)
        {
            if (!this.IsDisposed)
            {
                string msg = DateTime.Now.ToString("mm:ss.fff") + " " + log + "\r\n\r\n";

                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        textBox1.AppendText(msg);
                    }));
                }
                else
                {
                    textBox1.AppendText(msg);
                }
            }
        }
        #endregion

        #region 清空输出框
        private void button10_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
        }
        #endregion

        #region RunTask
        private Task RunTask(Action action)
        {
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    throw;
                }
            });
        }
        #endregion

        #region 删除
        private void button5_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                Log("删除 开始");
                var session = LiteSqlFactory.GetSession();
                session.DeleteByCondition<SysUser>(string.Format("id>=12"));
                Log("删除 完成");
            });
        }
        #endregion

        #region 测试批量修改
        private void button3_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                List<SysUser> userList = m_SysUserDal.GetList("select t.* from sys_user t where t.id > 20");

                Log("批量修改 开始 count=" + userList.Count);
                DateTime dt = DateTime.Now;

                var session = LiteSqlFactory.GetSession();

                try
                {
                    session.OnExecuting = (sql, param) =>
                    {
                        Console.WriteLine(sql); //打印SQL
                    };

                    session.AttachOld(userList);
                    foreach (SysUser user in userList)
                    {
                        user.Remark = "测试修改用户" + _rnd.Next(1, 10000);
                        user.UpdateUserid = "1";
                        user.UpdateTime = DateTime.Now;
                    }
                    userList.ForEach(item => item.UpdateTime = DateTime.Now);

                    session.BeginTransaction();
                    session.Update(userList);
                    session.CommitTransaction();
                }
                catch (Exception ex)
                {
                    session.RollbackTransaction();
                    throw;
                }

                string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
                Log("批量修改 完成，耗时：" + time + "秒");
            });
        }
        #endregion

        #region 测试批量添加
        private void button4_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                List<SysUser> userList = new List<SysUser>();
                for (int i = 1; i <= _count; i++)
                {
                    SysUser user = new SysUser();
                    user.UserName = "testUser";
                    user.RealName = "测试插入用户";
                    user.Password = "123456";
                    user.CreateUserid = "1";
                    user.CreateTime = DateTime.Now;
                    userList.Add(user);
                }

                Log("批量添加 开始 count=" + userList.Count);
                DateTime dt = DateTime.Now;

                var session = LiteSqlFactory.GetSession();

                try
                {
                    session.OnExecuting = (sql, param) =>
                    {
                        Console.WriteLine(sql); //打印SQL
                    };

                    session.BeginTransaction();
                    session.Insert(userList);
                    session.CommitTransaction();
                }
                catch
                {
                    session.RollbackTransaction();
                    throw;
                }

                string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
                Log("批量添加 完成，耗时：" + time + "秒");
            });
        }
        #endregion

        #region 测试循环修改
        private void button7_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                List<SysUser> userList = m_SysUserDal.GetList("select t.* from sys_user t where t.id > 20");

                Log("循环修改 开始 count=" + userList.Count);
                DateTime dt = DateTime.Now;

                var session = LiteSqlFactory.GetSession();

                try
                {
                    session.OnExecuting = (sql, param) =>
                    {
                        Console.WriteLine(sql); //打印SQL
                    };

                    session.AttachOld(userList);
                    foreach (SysUser user in userList)
                    {
                        user.Remark = "测试修改用户" + _rnd.Next(1, 10000);
                        user.UpdateUserid = "1";
                        user.UpdateTime = DateTime.Now;
                    }

                    session.BeginTransaction();
                    foreach (SysUser user in userList)
                    {
                        session.Update(user);
                    }
                    session.CommitTransaction();
                }
                catch
                {
                    session.RollbackTransaction();
                    throw;
                }


                string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
                Log("循环修改 完成，耗时：" + time + "秒");
            });
        }
        #endregion

        #region 测试循环添加
        private void button6_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                List<SysUser> userList = new List<SysUser>();
                for (int i = 1; i <= _count; i++)
                {
                    SysUser user = new SysUser();
                    user.UserName = "testUser";
                    user.RealName = "测试插入用户";
                    user.Password = "123456";
                    user.CreateUserid = "1";
                    userList.Add(user);
                }

                Log("循环添加 开始 count=" + userList.Count);
                DateTime dt = DateTime.Now;

                var session = LiteSqlFactory.GetSession();

                try
                {
                    session.OnExecuting = (sql, param) =>
                    {
                        Console.WriteLine(sql); //打印SQL
                    };

                    session.BeginTransaction();
                    foreach (SysUser user in userList)
                    {
                        session.Insert(user);
                    }
                    session.CommitTransaction();
                }
                catch
                {
                    session.RollbackTransaction();
                    throw;
                }

                string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
                Log("循环添加 完成，耗时：" + time + "秒");
            });
        }
        #endregion

        #region 查询
        private void button1_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                Log("查询 开始");
                DateTime dt = DateTime.Now;

                for (int i = 0; i < 10; i++)
                {
                    var session = LiteSqlFactory.GetSession();

                    ISqlString sql = session.CreateSql(@"
                        select t.* 
                        from sys_user t 
                        where t.id > @id 
                        and t.real_name like @remark", 20, "%测试%");

                    sql.Append(" order by t.create_time desc, t.id asc");

                    List<SysUser> userList = sql.QueryList<SysUser>();
                    Log("查询结果 count=" + userList.Count.ToString());
                }

                string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
                Log("查询 完成，耗时：" + time + "秒");
            });
        }
        #endregion

        #region 分页查询
        private void button2_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                Log("分页查询 开始");
                DateTime dt = DateTime.Now;

                for (int i = 0; i < 10; i++)
                {
                    int total = m_SysUserDal.GetTotalCount();
                    int pageSize = 100;
                    int pageCount = (total - 1) / pageSize + 1;
                    var session = LiteSqlFactory.GetSession();

                    List<SysUser> userList = new List<SysUser>();
                    for (int page = 1; page <= pageCount; page++)
                    {
                        ISqlString sql = session.CreateSql(@"
                            select t.* 
                            from sys_user t 
                            where 1=1 
                            and t.id > @id 
                            and t.real_name like @remark", 20, "%测试%");

                        string orderby = " order by t.create_time desc, t.id asc";

                        userList.AddRange(sql.QueryPage<SysUser>(orderby, pageSize, page));
                    }
                    Log("分页查询结果 count=" + userList.Count.ToString());
                }

                string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
                Log("分页查询 完成，耗时：" + time + "秒");
            });
        }
        #endregion

        #region 并发查询
        private void button8_Click(object sender, EventArgs e)
        {
            RunTask(() =>
            {
                Log("查询 开始");
                DateTime dt = DateTime.Now;

                List<Task> tasks = new List<Task>();
                for (int i = 0; i < 200; i++)
                {
                    Task task = RunTask(() =>
                    {
                        var session = LiteSqlFactory.GetSession();

                        ISqlString sql = session.CreateSql(@"
                            select t.* 
                            from sys_user t 
                            where t.id > @id 
                            and t.real_name like @remark", 20, "%测试%");

                        sql.Append(" order by t.create_time desc, t.id asc");

                        List<SysUser> userList = sql.QueryList<SysUser>();
                        Log("查询结果 count=" + userList.Count.ToString());
                    });
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());

                string time = DateTime.Now.Subtract(dt).TotalSeconds.ToString("0.000");
                Log("查询 完成，耗时：" + time + "秒");
            });
        }
        #endregion

    }
}
