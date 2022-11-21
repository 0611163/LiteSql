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
        public static Func<IDataRecord, T> BindData<T>(PropertyInfoEx[] propertyInfoList, Dictionary<string, int> fields, Dictionary<string, Type> fieldTypes, string strFields)
        {
            Type entityType = typeof(T);
            string key = entityType.FullName + "_" + strFields + "_T";

            if (_cacheDict.TryGetValue(key, out _))
            {
                return _cacheDict[key] as Func<IDataRecord, T>;
            }
            else
            {
                CreateBindings(entityType, propertyInfoList, fields, fieldTypes, out ParameterExpression dataRecordExpr, out Expression initExpr);

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
        public static Func<IDataRecord, object> BindData(Type entityType, PropertyInfoEx[] propertyInfoList, Dictionary<string, int> fields, Dictionary<string, Type> fieldTypes, string strFields)
        {
            string key = entityType.FullName + "_" + strFields + "_Type";
            if (_cacheDict.TryGetValue(key, out _))
            {
                return _cacheDict[key] as Func<IDataRecord, object>;
            }
            else
            {
                CreateBindings(entityType, propertyInfoList, fields, fieldTypes, out ParameterExpression dataRecordExpr, out Expression initExpr);

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
        private static void CreateBindings(Type entityType, PropertyInfoEx[] propertyInfoList, Dictionary<string, int> fields, Dictionary<string, Type> fieldTypes, out ParameterExpression dataRecordExpr, out Expression initExpr)
        {
            dataRecordExpr = Expression.Parameter(typeof(IDataRecord), "r");

            List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (PropertyInfoEx propertyInfoEx in propertyInfoList)
            {
                if (!fields.ContainsKey(propertyInfoEx.FieldNameUpper)) continue;

                Expression propertyValue = GetMethodCall(dataRecordExpr, propertyInfoEx, fields[propertyInfoEx.FieldNameUpper], fieldTypes[propertyInfoEx.FieldNameUpper]);

                MemberBinding binding = Expression.Bind(propertyInfoEx.PropertyInfo, propertyValue);
                bindings.Add(binding);
            }

            initExpr = Expression.MemberInit(Expression.New(entityType), bindings);
        }
        #endregion

        #region GetMethodCall
        private static Expression GetMethodCall(ParameterExpression dataRecordExpr, PropertyInfoEx propertyInfoEx, int fieldIndex, Type fieldType)
        {
            Type propertyType = propertyInfoEx.PropertyInfo.PropertyType;
            Type typeIDataRecord = typeof(IDataRecord);

            string methodName = GetIDataRecordMethod(ref fieldType);

            MethodCallExpression getValueExpr = Expression.Call(dataRecordExpr, typeIDataRecord.GetMethod(methodName), Expression.Constant(fieldIndex));

            MethodCallExpression isDBNullExpr = Expression.Call(dataRecordExpr, typeIDataRecord.GetMethod("IsDBNull"), Expression.Constant(fieldIndex));

            var convertExpr = GetConvertExpr(propertyType, fieldType, getValueExpr);

            return Expression.Condition(isDBNullExpr, Expression.Default(propertyType), convertExpr);
        }
        #endregion

        #region GetConvertExpr
        private static Expression GetConvertExpr(Type propertyType, Type fieldType, Expression getValueExpr)
        {
            Expression convertExpr = null;

            Type genericType = null;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                genericType = propertyType.GenericTypeArguments[0];
            }

            string methodName = GetConvertMethod(propertyType, out Type methodReturnType);

            if (propertyType != fieldType)
            {
                if (genericType != null)
                {
                    if (genericType == typeof(Guid))
                    {
                        Expression newGuidExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(string) }), getValueExpr);

                        convertExpr = Expression.Convert(newGuidExpr, propertyType);
                    }
                    else if (genericType != fieldType)
                    {
                        convertExpr = Expression.Call(typeof(Convert).GetMethod(methodName, new Type[] { fieldType }), getValueExpr);
                        convertExpr = Expression.Convert(convertExpr, propertyType);
                    }
                    else
                    {
                        convertExpr = Expression.Convert(getValueExpr, propertyType);
                    }
                }
                else
                {
                    if (propertyType == typeof(Guid))
                    {
                        Expression newGuidExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { typeof(string) }), getValueExpr);

                        convertExpr = newGuidExpr;
                    }
                    else if (methodName != null && methodReturnType != fieldType)
                    {
                        convertExpr = Expression.Call(typeof(Convert).GetMethod(methodName, new Type[] { fieldType }), getValueExpr);

                        if (propertyType != methodReturnType)
                        {
                            convertExpr = Expression.Convert(convertExpr, propertyType);
                        }
                    }
                    else
                    {
                        if (propertyType != methodReturnType || propertyType == typeof(byte[]))
                        {
                            convertExpr = Expression.Convert(getValueExpr, propertyType);
                        }
                        else if (propertyType != methodReturnType || propertyType.BaseType == typeof(Array))
                        {
                            convertExpr = Expression.Convert(getValueExpr, propertyType);
                        }
                        else
                        {
                            convertExpr = getValueExpr;
                        }
                    }
                }
            }
            else
            {
                convertExpr = getValueExpr;
            }

            return convertExpr;
        }
        #endregion

        #region GetIDataRecordMethod 获取IDataRecord方法名称和返回值类型
        /// <summary>
        /// 获取IDataRecord方法名称和返回值类型
        /// </summary>
        private static string GetIDataRecordMethod(ref Type fieldType)
        {
            string methodName = "GetValue";

            if (fieldType == typeof(string))
            {
                methodName = "GetString";
            }
            else if (fieldType == typeof(object))
            {
                methodName = "GetValue";
            }
            else if (fieldType == typeof(byte[]))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(Guid))
            {
                methodName = "GetString";
                fieldType = typeof(string);
            }
            else if (fieldType == typeof(char))
            {
                methodName = "GetChar";
            }
            else if (fieldType == typeof(byte))
            {
                methodName = "GetByte";
            }
            else if (fieldType == typeof(sbyte))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(short))
            {
                methodName = "GetInt16";
            }
            else if (fieldType == typeof(ushort))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(int))
            {
                methodName = "GetInt32";
            }
            else if (fieldType == typeof(uint))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(long))
            {
                methodName = "GetInt64";
            }
            else if (fieldType == typeof(ulong))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(float))
            {
                methodName = "GetFloat";
            }
            else if (fieldType == typeof(double))
            {
                methodName = "GetDouble";
            }
            else if (fieldType == typeof(decimal))
            {
                methodName = "GetDecimal";
            }
            else if (fieldType == typeof(bool))
            {
                methodName = "GetBoolean";
            }
            else if (fieldType == typeof(DateTime))
            {
                methodName = "GetDateTime";
            }
            // ======== 以下是可空类型 ================================ 
            else if (fieldType == typeof(Guid?))
            {
                methodName = "GetString";
                fieldType = typeof(string);
            }
            else if (fieldType == typeof(char?))
            {
                methodName = "GetChar";
                fieldType = typeof(char);
            }
            else if (fieldType == typeof(byte?))
            {
                methodName = "GetByte";
                fieldType = typeof(byte);
            }
            else if (fieldType == typeof(sbyte?))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(short?))
            {
                methodName = "GetInt16";
                fieldType = typeof(short);
            }
            else if (fieldType == typeof(ushort?))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(int?))
            {
                methodName = "GetInt32";
                fieldType = typeof(int);
            }
            else if (fieldType == typeof(uint?))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(long?))
            {
                methodName = "GetInt64";
                fieldType = typeof(long);
            }
            else if (fieldType == typeof(ulong?))
            {
                methodName = "GetValue";
                fieldType = typeof(object);
            }
            else if (fieldType == typeof(float?))
            {
                methodName = "GetFloat";
                fieldType = typeof(float);
            }
            else if (fieldType == typeof(double?))
            {
                methodName = "GetDouble";
                fieldType = typeof(double);
            }
            else if (fieldType == typeof(decimal?))
            {
                methodName = "GetDecimal";
                fieldType = typeof(decimal);
            }
            else if (fieldType == typeof(bool?))
            {
                methodName = "GetBoolean";
                fieldType = typeof(bool);
            }
            else if (fieldType == typeof(DateTime?))
            {
                methodName = "GetDateTime";
                fieldType = typeof(DateTime);
            }

            return methodName;
        }
        #endregion

        #region GetConvertMethod 获取Convert类的方法名称和返回值类型
        /// <summary>
        /// 获取Convert类的方法名称和返回值类型
        /// </summary>
        private static string GetConvertMethod(Type propertyType, out Type methodReturnType)
        {
            string methodName = null;
            methodReturnType = propertyType;

            if (propertyType == typeof(string))
            {
                methodName = "ToString";
            }
            else if (propertyType == typeof(char))
            {
                methodName = "ToChar";
            }
            else if (propertyType == typeof(byte))
            {
                methodName = "ToByte";
            }
            else if (propertyType == typeof(sbyte))
            {
                methodName = "ToByte";
                methodReturnType = typeof(byte);
            }
            else if (propertyType == typeof(short))
            {
                methodName = "ToInt16";
            }
            else if (propertyType == typeof(ushort))
            {
                methodName = "ToInt16";
                methodReturnType = typeof(short);
            }
            else if (propertyType == typeof(int))
            {
                methodName = "ToInt32";
            }
            else if (propertyType == typeof(uint))
            {
                methodName = "ToInt32";
                methodReturnType = typeof(int);
            }
            else if (propertyType == typeof(long))
            {
                methodName = "ToInt64";
            }
            else if (propertyType == typeof(ulong))
            {
                methodName = "ToInt64";
                methodReturnType = typeof(long);
            }
            else if (propertyType == typeof(float))
            {
                methodName = "ToSingle";
            }
            else if (propertyType == typeof(double))
            {
                methodName = "ToDouble";
            }
            else if (propertyType == typeof(decimal))
            {
                methodName = "ToDecimal";
            }
            else if (propertyType == typeof(bool))
            {
                methodName = "ToBoolean";
            }
            else if (propertyType == typeof(DateTime))
            {
                methodName = "ToDateTime";
            }
            // ======== 以下是可空类型 ================================ 
            else if (propertyType == typeof(char?))
            {
                methodName = "ToChar";
            }
            else if (propertyType == typeof(byte?))
            {
                methodName = "ToByte";
            }
            else if (propertyType == typeof(sbyte?))
            {
                methodName = "ToByte";
                methodReturnType = typeof(byte);
            }
            else if (propertyType == typeof(short?))
            {
                methodName = "ToInt16";
            }
            else if (propertyType == typeof(ushort?))
            {
                methodName = "ToInt16";
                methodReturnType = typeof(short);
            }
            else if (propertyType == typeof(int?))
            {
                methodName = "ToInt32";
            }
            else if (propertyType == typeof(uint?))
            {
                methodName = "ToInt32";
                methodReturnType = typeof(int);
            }
            else if (propertyType == typeof(long?))
            {
                methodName = "ToInt64";
            }
            else if (propertyType == typeof(ulong?))
            {
                methodName = "ToInt64";
                methodReturnType = typeof(long);
            }
            else if (propertyType == typeof(float?))
            {
                methodName = "ToSingle";
            }
            else if (propertyType == typeof(double?))
            {
                methodName = "ToDouble";
            }
            else if (propertyType == typeof(decimal?))
            {
                methodName = "ToDecimal";
            }
            else if (propertyType == typeof(bool?))
            {
                methodName = "ToBoolean";
            }
            else if (propertyType == typeof(DateTime?))
            {
                methodName = "ToDateTime";
            }

            return methodName;
        }
        #endregion

    }
}
