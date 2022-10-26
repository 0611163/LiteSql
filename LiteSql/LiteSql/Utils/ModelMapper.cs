using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// 相同类型的实体类映射
    /// </summary>
    public static class ModelMapper<T>
    {
        private static Func<T, T> _func = null;

        static ModelMapper()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "t");

            List<MemberBinding> memberBindings = new List<MemberBinding>();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                if (propertyInfo.CanWrite)
                {
                    MemberExpression propertyExpr = Expression.Property(parameterExpression, propertyInfo);
                    MemberBinding memberBinding = Expression.Bind(propertyInfo, propertyExpr);
                    memberBindings.Add(memberBinding);
                }
            }

            MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(T)), memberBindings);

            Expression<Func<T, T>> lamada = Expression.Lambda<Func<T, T>>(memberInitExpression, parameterExpression);
            _func = lamada.Compile();
        }

        /// <summary>
        /// 实体类映射
        /// </summary>
        public static object Map(T source)
        {
            return _func.Invoke(source);
        }

    }
}
