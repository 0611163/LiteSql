using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// IDataRecord到实体类的映射
    /// </summary>
    internal class ExpressionMapper
    {
        #region 变量
        /// <summary>
        /// 缓存
        /// </summary>
        private static ConcurrentDictionary<string, object> _cacheDict = new ConcurrentDictionary<string, object>();
        #endregion

        #region BindData<T> 数据绑定
        /// <summary>
        /// 数据绑定
        /// </summary>
        public static Func<IDataRecord, T> BindData<T>(PropertyInfoEx[] propertyInfoList, Dictionary<string, int> fields, string strFields)
        {
            Type entityType = typeof(T);
            string key = entityType.FullName + "_" + strFields + "_T";

            if (_cacheDict.TryGetValue(key, out _))
            {
                return _cacheDict[key] as Func<IDataRecord, T>;
            }
            else
            {
                CreateBindings(entityType, propertyInfoList, fields, out ParameterExpression dataRecordExpr, out Expression initExpr);

                Expression<Func<IDataRecord, T>> lambda = Expression.Lambda<Func<IDataRecord, T>>(initExpr, dataRecordExpr);

                var func = lambda.Compile();
                _cacheDict.TryAdd(key, func);
                return func;
            }

        }
        #endregion

        #region BindData 数据绑定
        /// <summary>
        /// 数据绑定
        /// </summary>
        public static Func<IDataRecord, object> BindData(Type entityType, PropertyInfoEx[] propertyInfoList, Dictionary<string, int> fields, string strFields)
        {
            string key = entityType.FullName + "_" + strFields + "_Type";
            if (_cacheDict.TryGetValue(key, out _))
            {
                return _cacheDict[key] as Func<IDataRecord, object>;
            }
            else
            {
                CreateBindings(entityType, propertyInfoList, fields, out ParameterExpression dataRecordExpr, out Expression initExpr);

                Expression<Func<IDataRecord, object>> lambda = Expression.Lambda<Func<IDataRecord, object>>(initExpr, dataRecordExpr);

                var func = lambda.Compile();
                _cacheDict.TryAdd(key, func);
                return func;
            }

        }
        #endregion

        #region CreateBindings 属性绑定
        /// <summary>
        /// 属性绑定
        /// </summary>
        private static void CreateBindings(Type entityType, PropertyInfoEx[] propertyInfoList, Dictionary<string, int> fields, out ParameterExpression dataRecordExpr, out Expression initExpr)
        {
            dataRecordExpr = Expression.Parameter(typeof(IDataRecord), "r");

            List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (PropertyInfoEx propertyInfoEx in propertyInfoList)
            {
                if (!fields.ContainsKey(propertyInfoEx.FieldNameUpper)) continue;

                Expression propertyValue = GetMethodCall(dataRecordExpr, propertyInfoEx, fields[propertyInfoEx.FieldNameUpper]);

                MemberBinding binding = Expression.Bind(propertyInfoEx.PropertyInfo, propertyValue);
                bindings.Add(binding);
            }

            initExpr = Expression.MemberInit(Expression.New(entityType), bindings);
        }
        #endregion

        #region GetMethodCall
        private static Expression GetMethodCall(ParameterExpression dataRecordExpr, PropertyInfoEx propertyInfoEx, int fieldIndex)
        {
            Type propertyType = propertyInfoEx.PropertyInfo.PropertyType;
            Type typeIDataRecord = typeof(IDataRecord);

            GetIDataRecordMethod(propertyType, out string methodName, out bool convert);

            MethodCallExpression getValueExpr = Expression.Call(dataRecordExpr, typeIDataRecord.GetMethod(methodName), Expression.Constant(fieldIndex));

            MethodCallExpression isDBNullExpr = Expression.Call(dataRecordExpr, typeIDataRecord.GetMethod("IsDBNull"), Expression.Constant(fieldIndex));

            if (convert)
            {
                if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                {
                    Expression guidExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(string) }), getValueExpr);

                    if (propertyType == typeof(Guid))
                    {
                        return Expression.Condition(isDBNullExpr, Expression.Default(propertyType), guidExpr);
                    }
                    else
                    {
                        var convertExpr = Expression.Convert(guidExpr, propertyType);

                        return Expression.Condition(isDBNullExpr, Expression.Default(propertyType), convertExpr);
                    }
                }
                else
                {
                    var convertExpr = Expression.Convert(getValueExpr, propertyType);

                    return Expression.Condition(isDBNullExpr, Expression.Default(propertyType), convertExpr);
                }
            }
            else
            {
                return getValueExpr;
            }
        }
        #endregion

        #region GetIDataRecordMethod
        private static void GetIDataRecordMethod(Type propertyType, out string methodName, out bool convert)
        {
            methodName = "GetString";
            convert = false;

            if (propertyType == typeof(string))
            {
                methodName = "GetString";
                convert = true;
            }
            else if (propertyType == typeof(object))
            {
                methodName = "GetValue";
                convert = true;
            }
            else if (propertyType == typeof(byte[]))
            {
                methodName = "GetValue";
                convert = true;
            }
            else if (propertyType == typeof(Guid))
            {
                methodName = "GetString";
                convert = true;
            }
            else if (propertyType == typeof(char))
            {
                methodName = "GetChar";
            }
            else if (propertyType == typeof(byte))
            {
                methodName = "GetByte";
            }
            else if (propertyType == typeof(sbyte))
            {
                methodName = "GetByte";
            }
            else if (propertyType == typeof(short))
            {
                methodName = "GetInt16";
            }
            else if (propertyType == typeof(ushort))
            {
                methodName = "GetInt16";
            }
            else if (propertyType == typeof(int))
            {
                methodName = "GetInt32";
            }
            else if (propertyType == typeof(uint))
            {
                methodName = "GetInt32";
            }
            else if (propertyType == typeof(long))
            {
                methodName = "GetInt64";
            }
            else if (propertyType == typeof(ulong))
            {
                methodName = "GetInt64";
            }
            else if (propertyType == typeof(float))
            {
                methodName = "GetFloat";
            }
            else if (propertyType == typeof(double))
            {
                methodName = "GetDouble";
            }
            else if (propertyType == typeof(decimal))
            {
                methodName = "GetDecimal";
            }
            else if (propertyType == typeof(bool))
            {
                methodName = "GetBoolean";
            }
            else if (propertyType == typeof(DateTime))
            {
                methodName = "GetDateTime";
            }
            // ======== 以下是可空类型 ================================ 
            else if (propertyType == typeof(Guid?))
            {
                methodName = "GetString";
                convert = true;
            }
            else if (propertyType == typeof(char?))
            {
                methodName = "GetChar";
                convert = true;
            }
            else if (propertyType == typeof(byte?))
            {
                methodName = "GetByte";
                convert = true;
            }
            else if (propertyType == typeof(sbyte?))
            {
                methodName = "GetByte";
                convert = true;
            }
            else if (propertyType == typeof(short?))
            {
                methodName = "GetInt16";
                convert = true;
            }
            else if (propertyType == typeof(ushort?))
            {
                methodName = "GetInt16";
                convert = true;
            }
            else if (propertyType == typeof(int?))
            {
                methodName = "GetInt32";
                convert = true;
            }
            else if (propertyType == typeof(uint?))
            {
                methodName = "GetInt32";
                convert = true;
            }
            else if (propertyType == typeof(long?))
            {
                methodName = "GetInt64";
                convert = true;
            }
            else if (propertyType == typeof(ulong?))
            {
                methodName = "GetInt64";
                convert = true;
            }
            else if (propertyType == typeof(float?))
            {
                methodName = "GetFloat";
                convert = true;
            }
            else if (propertyType == typeof(double?))
            {
                methodName = "GetDouble";
                convert = true;
            }
            else if (propertyType == typeof(decimal?))
            {
                methodName = "GetDecimal";
                convert = true;
            }
            else if (propertyType == typeof(bool?))
            {
                methodName = "GetBoolean";
                convert = true;
            }
            else if (propertyType == typeof(DateTime?))
            {
                methodName = "GetDateTime";
                convert = true;
            }
        }
        #endregion

    }
}
