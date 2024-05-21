using AlmostOrm.MapConfig;
using System.Text;
using System.Text.Json;

namespace AlmostOrm
{
    public static class MapGen
    {
        private static string path = "map-config.json";
        private static AlmostOrmSettings _settings;
        static MapGen()
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
            var name = typeof(T).Name;

            var tableName = config.TableName ?? 
                config.CaseConverter?.Convert(name) ??
                nameof(name);

            string id = _settings.IdTemplate;
            string index = string.Empty;


            var type = typeof(T);

            var tableContents = string.Join(",\n\t", GetMappedStrings(config));

            if (config.Index?.Any() == true)
            {
                index = CreateIndex(config, tableName, config.IsUniqueIndex, config.Index);
            }

            var result = new StringBuilder(_settings.TableTemplate)
                .Replace("<table_name>", tableName)
                .Replace("<table_contents>", tableContents)
                .Replace("<id>", id)
                .Replace("<index>", index);

            Console.WriteLine(result.ToString());
        }

        private static string CreateIndex<T>(MapConfig<T> config, string tableName, bool isUniqueIndex, string[] index) where T : class
        {
            if (index.Any(q => !config.Mapping.ContainsKey(q)))
            {
                throw new ArgumentException($"{typeof(T).Name} does not contain such properties");
            }

            if (config.CaseConverter is not null)
            {
                index = index
                    .Select(q => config.Mapping[q].CustomName ?? config.CaseConverter.Convert(q))
                    .ToArray();
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

        private static IEnumerable<string> GetMappedStrings<T>(MapConfig<T> config) where T : class
        {
            var mapping = config.Mapping;

            return mapping.Values.Select(map =>
            {
                var name = map.CustomName ??
                    config.CaseConverter?.Convert(map.Name) ??
                    map.Name;

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