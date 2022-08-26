using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    public partial interface ISession
    {
        /// <summary>
        /// 添加
        /// </summary>
        void Insert(object obj);

        /// <summary>
        /// 添加
        /// </summary>
        Task InsertAsync(object obj);

        /// <summary>
        /// 批量添加
        /// </summary>
        void Insert<T>(List<T> obj);

        /// <summary>
        /// 批量添加
        /// </summary>
        Task InsertAsync<T>(List<T> obj);
    }
}
