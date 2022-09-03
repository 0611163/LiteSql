using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteSql
{
    /// <summary>
    /// 表达式树解析
    /// </summary>
    public class ExpressionHelper<T>
    {
        #region 变量
        private ISqlString _sqlString;
        private IProvider _provider;
        private HashSet<string> _dbParameterNames;
        private SqlStringMethod _SqlStringMethod;
        #endregion

        #region 构造函数
        public ExpressionHelper(ISqlString sqlString, IProvider provider, HashSet<string> dbParameterNames, SqlStringMethod sqlStringMethod)
        {
            _sqlString = sqlString;
            _provider = provider;
            _dbParameterNames = dbParameterNames;
            _SqlStringMethod = sqlStringMethod;
        }
        #endregion

        #region VisitLambda
        /// <summary>
        /// VisitLambda
        /// </summary>
        public string VisitLambda(Expression exp, out DbParameter[] dbParameters)
        {
            LambdaExpression lambdaExp = exp as LambdaExpression;
            if (lambdaExp.Body is UnaryExpression)
            {
                UnaryExpression unaryExp = lambdaExp.Body as UnaryExpression;

                return VisitLevel1(unaryExp, out dbParameters);
            }
            else if (lambdaExp.Body is MemberExpression)
            {
                MemberExpression MemberExp = lambdaExp.Body as MemberExpression;

                ExpValue expValue = VisitLevel3(MemberExp);

                dbParameters = expValue.DbParameters.ToArray();
                return expValue.Sql;
            }
            else
            {
                throw new Exception("不支持");
            }
        }
        #endregion

        #region VisitLevel1 第一级
        /// <summary>
        /// 第一级
        /// </summary>
        public string VisitLevel1(UnaryExpression exp, out DbParameter[] dbParameters)
        {
            string result = string.Empty;

            ExpValue expValue = VisitLevel3(exp.Operand);

            result = expValue.Sql;
            dbParameters = expValue.DbParameters.ToArray();

            return result;
        }
        #endregion

        #region VisitLevel2 第二级 条件集合
        /// <summary>
        /// 第一级
        /// </summary>
        public ExpValue VisitLevel2(BinaryExpression exp)
        {
            ExpValue result = new ExpValue();

            ExpValue left = VisitLevel3(exp.Left);
            ExpValue right = VisitLevel3(exp.Right);

            result.Sql = string.Format(" {0} {1} {2} ", left.Sql, ToSqlOperator(exp.NodeType), right.Sql);
            result.Type = ExpValueType.SqlAndDbParameter;

            result.DbParameters.AddRange(left.DbParameters);
            result.DbParameters.AddRange(right.DbParameters);

            return result;
        }
        #endregion

        #region VisitLevel3 第三级 单个条件
        /// <summary>
        /// 第二级
        /// </summary>
        public ExpValue VisitLevel3(Expression exp)
        {
            ExpValue result = new ExpValue();

            if (exp.NodeType == ExpressionType.AndAlso ||
                exp.NodeType == ExpressionType.And ||
                exp.NodeType == ExpressionType.OrElse ||
                exp.NodeType == ExpressionType.Or) //多条件，回到Level2
            {
                BinaryExpression binaryExp = exp as BinaryExpression;
                result = VisitLevel2(binaryExp);
            }
            else
            {
                if (exp.NodeType == ExpressionType.Call) // 例: t => t.Remark.Contains("订单")
                {
                    result = VisitMethodCall(exp as MethodCallExpression);
                }
                else if (exp.NodeType == ExpressionType.MemberAccess) // 支持 order by
                {
                    ExpValue expValue = VisitMember(exp as MemberExpression, null);
                    result.Sql = string.Format("{0}.{1}", expValue.MemberParentName, expValue.MemberDBField);
                }
                else if (exp.NodeType == ExpressionType.NotEqual ||
                     exp.NodeType == ExpressionType.GreaterThan ||
                     exp.NodeType == ExpressionType.GreaterThanOrEqual ||
                     exp.NodeType == ExpressionType.LessThan ||
                     exp.NodeType == ExpressionType.LessThanOrEqual ||
                     exp.NodeType == ExpressionType.Equal)
                {
                    result = VisitLevel3Binary(exp as BinaryExpression); // 例: t => t.Status == 0 例: t => t.OrderTime >= new DateTime(2020,1,1)
                }
                else if (exp.NodeType == ExpressionType.Not) //支持 not in
                {
                    UnaryExpression unaryExp = exp as UnaryExpression;
                    result = VisitMethodCall(unaryExp.Operand as MethodCallExpression, exp);
                }
                else
                {
                    throw new Exception("不支持");
                }
            }

            return result;
        }
        #endregion

        #region VisitLevel3Binary 第三级的 二元表达式
        /// <summary>
        /// 第三级的 二元表达式
        /// </summary>
        public ExpValue VisitLevel3Binary(BinaryExpression exp)
        {
            ExpValue result = new ExpValue();

            if (_SqlStringMethod == SqlStringMethod.LeftJoin)
            {
                ExpValue left = VisitMember(exp.Left);
                ExpValue right = VisitMember(exp.Right);

                result.Sql = string.Format("{0}.{1} = {2}.{3}", left.MemberParentName, left.MemberDBField, right.MemberParentName, right.MemberDBField);
                result.Type = ExpValueType.SqlAndDbParameter;
            }
            else
            {
                if (exp.NodeType == ExpressionType.Not ||
                    exp.NodeType == ExpressionType.NotEqual ||
                    exp.NodeType == ExpressionType.GreaterThan ||
                    exp.NodeType == ExpressionType.GreaterThanOrEqual ||
                    exp.NodeType == ExpressionType.LessThan ||
                    exp.NodeType == ExpressionType.LessThanOrEqual ||
                    exp.NodeType == ExpressionType.Equal)
                {
                    ExpValue left = VisitMember(exp.Left);
                    ExpValue right = VisitValue(exp.Right);

                    left.MemberAliasName = GetAliasName(left.MemberAliasName);
                    _dbParameterNames.Add(left.MemberAliasName);

                    if (right.Value != null && right.Value.GetType() == typeof(DateTime))
                    {
                        SqlValue sqlValue = _provider.ForDateTime((DateTime)right.Value);
                        Type parameterType = sqlValue.Value == null ? typeof(object) : sqlValue.Value.GetType();
                        string markKey = _provider.GetParameterName(left.MemberAliasName, parameterType);

                        result.DbParameters.Add(_provider.GetDbParameter(left.MemberAliasName, right.Value));
                        result.Sql = string.Format(" {0}.{1} {2} {3} ", left.MemberParentName, left.MemberDBField, ToSqlOperator(exp.NodeType), string.Format(sqlValue.Sql, markKey));
                    }
                    else
                    {
                        if (right.Value != null)
                        {
                            string markKey = _provider.GetParameterName(left.MemberAliasName, right.Value.GetType());
                            result.DbParameters.Add(_provider.GetDbParameter(left.MemberAliasName, right.Value));
                            result.Sql = string.Format(" {0}.{1} {2} {3} ", left.MemberParentName, left.MemberDBField, ToSqlOperator(exp.NodeType), markKey);
                        }
                        else
                        {
                            if (exp.NodeType == ExpressionType.Not ||
                                exp.NodeType == ExpressionType.NotEqual)
                            {
                                result.Sql = string.Format(" {0}.{1} is not null ", left.MemberParentName, left.MemberDBField);
                            }
                            else if (exp.NodeType == ExpressionType.Equal)
                            {
                                result.Sql = string.Format(" {0}.{1} is null ", left.MemberParentName, left.MemberDBField);
                            }
                            else
                            {
                                throw new Exception("不支持");
                            }
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region VisitMethodCall 方法
        /// <summary>
        /// 方法
        /// </summary>
        public ExpValue VisitMethodCall(MethodCallExpression exp, Expression parent = null)
        {
            ExpValue result = new ExpValue();

            if (exp.Method.Name == "Contains" ||
                exp.Method.Name == "StartsWith" ||
                exp.Method.Name == "EndsWith")
            {
                MemberExpression memberExp = exp.Object as MemberExpression;
                if (memberExp.Type.Name != typeof(List<>).Name)
                {
                    SqlValue sqlValue = null;
                    if (exp.Method.Name == "Contains") sqlValue = _sqlString.ForContains(InvokeValue(exp.Arguments[0]).ToString());
                    if (exp.Method.Name == "StartsWith") sqlValue = _sqlString.ForStartsWith(InvokeValue(exp.Arguments[0]).ToString());
                    if (exp.Method.Name == "EndsWith") sqlValue = _sqlString.ForEndsWith(InvokeValue(exp.Arguments[0]).ToString());
                    ExpValue expValue = VisitMember(exp.Object as MemberExpression, null);

                    expValue.MemberAliasName = GetAliasName(expValue.MemberAliasName);
                    _dbParameterNames.Add(expValue.MemberAliasName);

                    Type parameterType = sqlValue.Value.GetType();
                    string markKey = _provider.GetParameterName(expValue.MemberAliasName, parameterType);

                    string not = string.Empty;
                    if (parent != null && parent.NodeType == ExpressionType.Not) // not like
                    {
                        not = "not";
                    }

                    result.Sql = string.Format("{0}.{1} {2} like {3}", expValue.MemberParentName, expValue.MemberDBField, not, string.Format(sqlValue.Sql, markKey));
                    result.DbParameters.Add(_provider.GetDbParameter(expValue.MemberAliasName, sqlValue.Value));
                }
                else // 支持 in 和 not in 例: t => idList.Contains(t.Id)
                {
                    if (exp.Method.Name == "Contains")
                    {
                        SqlValue sqlValue = null;
                        sqlValue = _sqlString.ForList((IList)InvokeValue(exp.Object));

                        ExpValue expValue = VisitMember(exp.Arguments[0], null);

                        expValue.MemberAliasName = GetAliasName(expValue.MemberAliasName);
                        _dbParameterNames.Add(expValue.MemberAliasName);

                        Type parameterType = sqlValue.Value.GetType();
                        string markKey = _provider.GetParameterName(expValue.MemberAliasName, parameterType);

                        string inOrNotIn = string.Empty;
                        if (parent != null && parent.NodeType == ExpressionType.Not)
                        {
                            inOrNotIn = "not in";
                        }
                        else
                        {
                            inOrNotIn = "in";
                        }

                        result.Sql = string.Format("{0}.{1} {2} {3}", expValue.MemberParentName, expValue.MemberName, inOrNotIn, string.Format(sqlValue.Sql, markKey));

                        string[] keyArr = sqlValue.Sql.Replace("(", string.Empty).Replace(")", string.Empty).Replace("@", string.Empty).Split(',');
                        IList valueList = (IList)sqlValue.Value;
                        for (int k = 0; k < valueList.Count; k++)
                        {
                            object item = valueList[k];
                            result.DbParameters.Add(_provider.GetDbParameter(keyArr[k], item));
                        }
                    }
                    else
                    {
                        throw new Exception("不支持");
                    }
                }
            }
            else // 支持 ToString、Parse 等其它方法
            {
                result.Value = ReflectionValue(exp, null);
                result.Type = ExpValueType.OnlyValue;
            }

            return result;
        }
        #endregion

        #region VisitValue 取值
        /// <summary>
        /// 第一级
        /// </summary>
        public ExpValue VisitValue(Expression exp, MemberExpression parent = null)
        {
            ExpValue result = new ExpValue();

            if (exp.NodeType == ExpressionType.Call) // 例: t => t.Status == int.Parse("0") 例: t => t.OrderTime <= DateTime.Now.AddDays(1)
            {
                result = VisitMethodCall(exp as MethodCallExpression);
            }
            else if (exp.NodeType == ExpressionType.New) // 例: t => t.OrderTime > new DateTime(2020, 1, 1)
            {
                result = VisitNew(exp as NewExpression);
            }
            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberExp = exp as MemberExpression;
                if (memberExp.Expression is MemberExpression) // 支持对象变量的属性 例: t => t.OrderTime > time.startTime.Value 例: t => t.Remark.Contains(order.Remark)
                {
                    result = VisitValue(memberExp.Expression, memberExp);
                }
                else
                {
                    object obj = ReflectionValue(exp, parent); // 例: t => t.OrderTime < DateTime.Now  例: t => t.Remark.Contains(new BsOrder().Remark)
                    result.Value = obj;
                    result.Type = ExpValueType.OnlyValue;
                }
            }
            else if (exp.NodeType == ExpressionType.Constant) // 支持常量、null
            {
                result.Value = VisitConstant(exp);
                result.Type = ExpValueType.OnlyValue;
            }
            else if (exp.NodeType == ExpressionType.Convert) // 字段是可空类型的情况
            {
                result = VisitConvert(exp);
            }
            else
            {
                throw new Exception("不支持");
            }

            return result;
        }
        #endregion

        #region VisitMember 字段或属性
        /// <summary>
        /// 字段或属性
        /// </summary>
        public ExpValue VisitMember(Expression exp, MemberExpression parent = null)
        {
            ExpValue result = new ExpValue();

            if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression mebmerExp = exp as MemberExpression;
                if (mebmerExp.Expression is ParameterExpression) // 例: exp = t.Remark
                {
                    ParameterExpression parameterExp = mebmerExp.Expression as ParameterExpression;

                    result.MemberParentName = parameterExp.Name;
                    result.MemberDBField = GetDbField(mebmerExp.Member.Name, mebmerExp.Expression.Type);
                    result.MemberName = mebmerExp.Member.Name;
                    result.MemberAliasName = mebmerExp.Member.Name;
                    result.Type = ExpValueType.MemberValue;
                }
                else
                {
                    throw new Exception("不支持");
                }
            }
            else if (exp.NodeType == ExpressionType.Convert) //例：exp = t.OrderTime >= startTime (startTime的类型是可空类型DateTime?)
            {
                return VisitMember((exp as UnaryExpression).Operand);
            }
            else
            {
                throw new Exception("不支持");
            }

            return result;
        }
        #endregion

        #region InvokeValue
        public object InvokeValue(Expression exp)
        {
            object result = string.Empty;

            if (exp.NodeType == ExpressionType.Constant)  //常量
            {
                result = VisitConstant(exp);
            }
            else
            {
                result = Expression.Lambda(exp).Compile().DynamicInvoke();
            }

            return result;
        }
        #endregion

        #region ReflectionValue
        private object ReflectionValue(Expression member, MemberExpression parent)
        {
            object result = Expression.Lambda(member).Compile().DynamicInvoke();

            if (result != null && result.GetType().IsClass && result.GetType() != typeof(string) && parent != null)
            {
                result = Expression.Lambda(parent).Compile().DynamicInvoke();
            }

            return result;
        }
        #endregion

        #region VisitConstant 常量表达式
        /// <summary>
        /// 常量表达式
        /// </summary>
        private object VisitConstant(Expression exp)
        {
            if (exp is ConstantExpression)
            {
                ConstantExpression constantExp = exp as ConstantExpression;
                return constantExp.Value;
            }
            else
            {
                throw new Exception("不是ConstantExpression");
            }
        }
        #endregion

        #region VisitNew
        /// <summary>
        /// New 表达式
        /// </summary>
        public ExpValue VisitNew(NewExpression exp)
        {
            ExpValue result = new ExpValue();

            List<object> args = new List<object>();
            foreach (Expression argExp in exp.Arguments.ToArray())
            {
                args.Add(InvokeValue(argExp));
            }

            result.Value = exp.Constructor.Invoke(args.ToArray());
            result.Type = ExpValueType.OnlyValue;

            return result;
        }
        #endregion

        #region VisitConvert
        /// <summary>
        /// Convert 表达式
        /// </summary>
        public ExpValue VisitConvert(Expression exp)
        {
            ExpValue result;

            Expression operandExp = (exp as UnaryExpression).Operand;
            if (operandExp is UnaryExpression)
            {
                result = VisitValue((operandExp as UnaryExpression).Operand);
            }
            else if (operandExp is MemberExpression)
            {
                result = VisitValue(operandExp);
            }
            else
            {
                throw new Exception("不支持");
            }

            return result;
        }
        #endregion

        #region ToSqlOperator
        private string ToSqlOperator(ExpressionType type)
        {
            switch (type)
            {
                case (ExpressionType.AndAlso):
                case (ExpressionType.And):
                    return "AND";
                case (ExpressionType.OrElse):
                case (ExpressionType.Or):
                    return "OR";
                case (ExpressionType.Not):
                    return "NOT";
                case (ExpressionType.NotEqual):
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case (ExpressionType.Equal):
                    return "=";
                default:
                    throw new Exception("不支持该方法");
            }
        }
        #endregion

        #region GetDbField
        private string GetDbField(string name, Type type)
        {
            string result = string.Empty;

            foreach (PropertyInfoEx propertyInfoEx in DBSession.GetEntityProperties(type))
            {
                PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;

                if (propertyInfo.Name.ToUpper() == name.ToUpper())
                {
                    ColumnAttribute isDBFieldAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
                    if (isDBFieldAttribute != null && isDBFieldAttribute.FieldName != null)
                    {
                        return _provider.OpenQuote + isDBFieldAttribute.FieldName + _provider.CloseQuote;
                    }
                    else
                    {
                        return _provider.OpenQuote + propertyInfo.Name + _provider.CloseQuote;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(result))
            {

            }

            return result;
        }
        #endregion

        #region GetAliasName
        /// <summary>
        /// 获取不冲突的别名
        /// </summary>
        private string GetAliasName(string aliasName)
        {
            if (!_dbParameterNames.Contains(aliasName))
            {
                _dbParameterNames.Add(aliasName);
                return aliasName;
            }
            else
            {
                return GetAliasName(aliasName + "A");
            }
        }
        #endregion

    }
}
