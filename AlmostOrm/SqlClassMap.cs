using AlmostOrm.MapConfig;
using System.Linq.Expressions;
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
            var tableName = config?.TableName ?? nameof(T).ToLower();
            sb.Append(@$"CREATE TABLE {tableName}
(
");
            var type = typeof(T);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            sb.Append(string.Join(",\n", GetMappedStrings(properties)));
            sb.AppendLine("\n);");

            if (config?.Index?.Any() == true)
            {
                sb.Append(CreateIndex(type, tableName, config.IsUniqueIndex, config.Index));
            }

            Console.WriteLine(sb.ToString());
        }

        private static string CreateIndex(Type type, string tableName, bool isUniqueIndex, string[] index)
        {
            if (index.Any(q => type.GetProperty(q, BindingFlags.Public| BindingFlags.Instance) is null))
            {
                throw new ArgumentException($"{type.Name} does not contain such properties");
            }

            return @$"
CREATE {(isUniqueIndex ? "UNIQUE INDEX ux_" : "INDEX ix_")}{tableName}_{string.Join("_", index)}
ON {tableName}
(
    {string.Join(",\n    ", index)}
);
";
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