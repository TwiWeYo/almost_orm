using System.Linq.Expressions;
using System.Reflection;

namespace AlmostOrm
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

            return new ParaMap<T>() { Name = name, Type = propType }
                .IsNullable(isNullable);
        }
    }
    public sealed class ParaMap<T> where T: class
    {

        internal string Name { get; set; }
        internal Type Type { get; set; }
        public string? CustomName { get; private set; }
        public string? SqlType { get; private set; }
        public (uint, uint?)? Precision { get; private set; }
        public bool Nullable { get; private set; }

        public ParaMap<T> WithCustomName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            CustomName = name;
            return this;
        }

        public ParaMap<T> WithType(string type)
        {
            if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));

            SqlType = type;
            return this;
        }

        public ParaMap<T> WithPrecision(uint precision, uint? doublePrecision = null)
        {
            Precision = (precision, doublePrecision);
            return this;
        }

        public ParaMap<T> IsNullable(bool isNullable)
        {
            Nullable = isNullable;
            return this;
        }
    }
}
