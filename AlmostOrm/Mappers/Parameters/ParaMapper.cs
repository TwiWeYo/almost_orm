using System.Linq.Expressions;
using System.Reflection;

namespace AlmostOrm.Mappers.Parameters
{
    public static class ParaMapper
    {
        public static ParaMap<T> Map<T>(Expression<Func<T, object>> propertyLambda) where T : class
        {
            if (propertyLambda.Body is not MemberExpression member)
            {
                if (propertyLambda.Body is UnaryExpression expression && expression.NodeType == ExpressionType.Convert && expression.Operand is MemberExpression subMember)
                {
                    member = subMember;
                }

                else
                {
                    throw new ArgumentException(string.Format(
                        "Expression '{0}' refers to a method, not a property.",
                        propertyLambda.ToString()));
                }
            }

            if (member.Member is not PropertyInfo property)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));
            }

            Type type = typeof(T);
            if (property.ReflectedType != null && type != property.ReflectedType && !type.IsSubclassOf(property.ReflectedType))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));
            }

            var nullabilityInfoContext = new NullabilityInfoContext();

            var propType = property.PropertyType;
            var nullabilityInfo = nullabilityInfoContext.Create(property);

            var name = property.Name;
            var isNullable = false;

            if (nullabilityInfo.WriteState is NullabilityState.Nullable)
            {
                propType = propType.IsValueType ? Nullable.GetUnderlyingType(propType)! : propType;
                isNullable = true;
            }

            return new(name, propType, isNullable);
        }
    }
}
