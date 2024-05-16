using AlmostOrm.MapConfig;
using System.Reflection;
using System.Text.Json;

namespace AlmostOrm
{
    public static class SqlClassMap
    {
        private static string path = "map-config.json";
        private static AlmostOrmSettings _settings;
        static SqlClassMap()
        {
            using var fs = new FileStream(path, FileMode.Open);
            _settings = JsonSerializer.Deserialize<AlmostOrmSettings>(fs)!;
            if (_settings == null)
            {
                throw new ArgumentNullException(nameof(fs));
            }
        }

        public static void GenerateTestMap<T>(MapConfig<T>? config = null) where T : class
        {
            var tableName = config?.TableName ?? nameof(T).ToLower();
            string id = _settings.IdTemplate;
            string index = string.Empty;


            var type = typeof(T);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var tableContents = string.Join(",\n\t", GetMappedStrings(properties));

            if (config?.Index?.Any() == true)
            {
                index = CreateIndex(type, tableName, config.IsUniqueIndex, config.Index);
            }

            var result = _settings.TableTemplate
                .Replace("<table_name>", tableName)
                .Replace("<table_contents>", tableContents)
                .Replace("<id>", id)
                .Replace("<index>", index);

            Console.WriteLine(result);
        }

        private static string CreateIndex(Type type, string tableName, bool isUniqueIndex, string[] index)
        {
            if (index.Any(q => type.GetProperty(q, BindingFlags.Public| BindingFlags.Instance) is null))
            {
                throw new ArgumentException($"{type.Name} does not contain such properties");
            }
            var indexName = $"{(isUniqueIndex ? "ux_" : "ix_")}{tableName}_{string.Join('_', index)}";
            var indexContents = string.Join(",\n\t", index);

            return _settings.IndexTemplate
                .Replace("<unique>", isUniqueIndex ? "unique" : string.Empty)
                .Replace("<index_name>", indexName)
                .Replace("<table_name>", tableName)
                .Replace("<index_contents>", indexContents);
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
                var typeName = propType.Name.ToLower();
                if (_settings.TypeMaps.TryGetValue(typeName, out var sqlType))
                {
                    yield return $"{name} {sqlType}{additionalInfo}";
                }
                else
                {
                    throw new ArgumentException($"{propType.Name} is not supported");
                }
            }
        }
    }
}