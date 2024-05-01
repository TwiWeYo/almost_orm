using AlmostOrm.MapConfig;
using System.Reflection;
using System.Text;

namespace AlmostOrm
{
    public static class SqlClassMap
    {
        private static readonly Type _typeInt = typeof(int);
        private static readonly Type _typeLong = typeof(long);
        private static readonly Type _typeFloat = typeof(float);
        private static readonly Type _typeDouble = typeof(double);
        private static readonly Type _typeDecimal = typeof(decimal);
        private static readonly Type _typeString = typeof(string);
        public static void GenerateTestMap<T>(MapConfig<T>? config = null) where T : class
        {
            var sb = new StringBuilder();
            sb.Append(@$"CREATE TABLE {config?.TableName ?? nameof(T).ToLower()}
(
");
            var type = typeof(T);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            sb.Append(string.Join(",\n", GetMappedStrings(properties)));
            sb.AppendLine("\n);");

            Console.WriteLine(sb.ToString());
        }

        private static IEnumerable<string> GetMappedStrings(PropertyInfo[] properties)
        {
            var nullabilityInfoContext = new NullabilityInfoContext();

            foreach (var property in properties)
            {
                var propType = property.PropertyType;
                var nullabilityInfo = nullabilityInfoContext.Create(property);

                var name = property.Name;
                var additionalInfo = " NOT NULL";

                if (nullabilityInfo.WriteState is NullabilityState.Nullable)
                {
                    propType = propType.IsValueType ? Nullable.GetUnderlyingType(propType) : propType;

                    additionalInfo = string.Empty;
                }

                var sqlType = propType switch
                {
                    _ when propType == _typeInt => "int",
                    _ when propType == _typeLong => "bigint",
                    _ when propType == _typeFloat || propType == _typeDouble || propType == _typeDecimal => "numeric(12, 6)",
                    _ when propType == _typeString => "varchar(128)",
                    _ => throw new NotSupportedException($"{propType!.Name} is not yet supported")
                };

                yield return $"    {name} {sqlType}{additionalInfo}";
            }
        }
    }
}