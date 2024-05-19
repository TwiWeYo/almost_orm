using AlmostOrm.MapConfig;
using System.Reflection;
using System.Text;
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
            config ??= new();

            var tableName = config.TableName ?? nameof(T).ToLower();
            string id = _settings.IdTemplate;
            string index = string.Empty;


            var type = typeof(T);

            var tableContents = string.Join(",\n\t", GetMappedStrings(config.Mapping));

            if (config.Index?.Any() == true)
            {
                index = CreateIndex(type, tableName, config.IsUniqueIndex, config.Index);
            }

            var result = new StringBuilder(_settings.TableTemplate)
                .Replace("<table_name>", tableName)
                .Replace("<table_contents>", tableContents)
                .Replace("<id>", id)
                .Replace("<index>", index);

            Console.WriteLine(result.ToString());
        }

        private static string CreateIndex(Type type, string tableName, bool isUniqueIndex, string[] index)
        {
            if (index.Any(q => type.GetProperty(q, BindingFlags.Public| BindingFlags.Instance) is null))
            {
                throw new ArgumentException($"{type.Name} does not contain such properties");
            }
            var indexName = $"{(isUniqueIndex ? "ux_" : "ix_")}{tableName}_{string.Join('_', index)}";
            var indexContents = string.Join(",\n\t", index);

            return new StringBuilder(_settings.IndexTemplate)
                .Replace("<unique>", isUniqueIndex ? "unique" : string.Empty)
                .Replace("<index_name>", indexName)
                .Replace("<table_name>", tableName)
                .Replace("<index_contents>", indexContents)
                .ToString();
        }

        private static IEnumerable<string> GetMappedStrings<T>(Dictionary<string, ParaMap<T>> mapping) where T : class
        {
            return mapping.Values.Select(map =>
            {
                var name = map.CustomName ?? map.Name.ToLower();

                var precision = map.Precision?.Item1.ToString() ?? string.Empty;
                precision += map.Precision?.Item2?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(precision))
                    precision = $"({precision})";

                var type = map.SqlType ?? _settings.TypeMaps[map.Type.Name.ToLower()];
                type = type.Replace("<precision>", precision);
                var nullability = map.Nullable ? string.Empty : " not null";

                return $"{name} {type}{nullability}";
            });
        }
    }
}