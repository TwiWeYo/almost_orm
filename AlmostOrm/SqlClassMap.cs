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
        public static void GenerateTestMap<T>() where T : class
        {
            var type = typeof(T);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder();
            sb.Append(string.Join(",\n", GetMappedStrings(properties)));

            Console.WriteLine(sb.ToString());
        }

        private static IEnumerable<string> GetMappedStrings(PropertyInfo[] properties)
        {
            foreach (var property in properties)
            {
                var propType = property.PropertyType;
                var name = property.Name;

                var sqlType = propType switch
                {
                    _ when propType == _typeInt => "int",
                    _ when propType == _typeLong => "bigint",
                    _ when propType == _typeFloat || propType == _typeDouble || propType == _typeDecimal => "numeric(12, 6)",
                    _ when propType == _typeString => "varchar(128)",
                    _ => throw new NotSupportedException($"{propType.Name} is not yet supported")
                };

                yield return $"{name} {sqlType}";
            }
        }
    }
}