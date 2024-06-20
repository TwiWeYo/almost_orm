using AlmostOrm.Mappers;
using System.Text;
using System.Text.Json;

namespace AlmostOrm;

public static class MapGen
{
    private static string _path = "Config/map-config.json";
    private static Settings _settings;
    static MapGen()
    {
        using var fs = new FileStream(_path, FileMode.Open);
        _settings = JsonSerializer.Deserialize<Settings>(fs)!;
        if (_settings == null)
        {
            throw new ArgumentNullException(nameof(fs));
        }
    }

    public static void MakeTable<T>(string tablePath, MapConfig<T>? config = null) where T : class => MakeTable<T>(_ => tablePath, config);
    public static void MakeTable<T>(Func<string, string> tablePath, MapConfig<T>? config = null) where T : class
    {
        config ??= new();
        var name = typeof(T).Name;

        var tableName = config.CaseConverter?.Convert(name) ?? nameof(name);
        tableName = config.TableName?.Invoke(tableName) ?? tableName;

        var index = string.Empty;

        var tableContents = string.Join(",\n\t", GetMappedStrings(config));

        if (config.Index?.Any() == true)
        {
            index = CreateIndex(config, tableName);
        }

        var result = new StringBuilder(_settings.TableTemplate)
            .Replace("<table_name>", tableName)
            .Replace("<table_contents>", tableContents)
            .Replace("<index>", index)
            .Replace("\t", "    ");

        WriteToFile(tablePath.Invoke(tableName), result.ToString());
    }

    public static void MakeProcedure<T>(string procedurePath, MapConfig<T>? config = null) where T : class => MakeProcedure(_ => procedurePath, config);
    public static void MakeProcedure<T>(Func<string, string> procedurePath, MapConfig<T>? config = null) where T : class
    {
        config ??= new();
        var name = typeof(T).Name;

        var procedureName = config.CaseConverter?.Convert(name) ?? nameof(name);
        procedureName = config.ProcedureName?.Invoke(procedureName) ?? procedureName;

        var tableName = config.CaseConverter?.Convert(name) ?? nameof(name);
        tableName = config.TableName?.Invoke(tableName) ?? tableName;

        var index = config.Index;
        var isUniqueIndex = config.IsUniqueIndex;

        CheckIndex(config);

        if (config.CaseConverter is not null)
        {
            index = index?
                .Select(q => config.ParaMaps[q].CustomName ?? config.CaseConverter.Convert(q))
                .ToHashSet();
        }

        var indexContents = index?.Any() == true ? $"({string.Join(", ", index)})" : string.Empty;

        var procedureContents = string.Join(",\n\t", GetMappedStrings(config));

        var procedureData = config.ParaMaps.Values
            .Select
            (
                map =>
                map.CustomName ??
                config.CaseConverter?.Convert(map.Name) ??
                map.Name
            );

        var procedureValues = procedureData
            .Select(q => $"{procedureName}.{q}");

        var onConflict = config.OnConflict switch
        {
            OnConflict.DoNothing => $"ON CONFLICT{indexContents} DO NOTHING",
            OnConflict.Update => CreateOnConflict(procedureData, indexContents),
            _ => ";"
        };

        var result = new StringBuilder(_settings.ProcedureTemplate)
            .Replace("<procedure_name>", procedureName)
            .Replace("<procedure_contents>", procedureContents)
            .Replace("<table_name>", tableName)
            .Replace("<procedure_data>", string.Join(",\n\t\t", procedureData))
            .Replace("<procedure_values>", string.Join(",\n\t\t", procedureValues))
            .Replace("<on_conflict>", onConflict)
            .Replace("\t", "    ");

        WriteToFile(procedurePath.Invoke(procedureName), result.ToString());
    }

    private static void WriteToFile(string path, string result)
    {

        using (var fs = new FileStream(path, File.Exists(path) ? FileMode.Truncate : FileMode.Create))
        using (var sw = new StreamWriter(fs))
        {
            sw.Write(result);
        }
    }

    private static string CreateOnConflict(IEnumerable<string> procedureData, string index)
    {
        var sb = new StringBuilder();
        sb.Append($"ON CONFLICT{index} DO\n\tUPDATE SET\n\t\t");
        sb.Append(string.Join(",\n\t\t", procedureData.Select(q => $"{q} = excluded.{q}")));
        sb.Append(";");

        return sb.ToString();
    }

    private static void CheckIndex<T>(MapConfig<T> config) where T : class
    {
        if (config.Index.Any(q => !config.ParaMaps.ContainsKey(q)))
        {
            throw new ArgumentException($"{typeof(T).Name} does not contain such properties");
        }

        if (config.Index.Any(q => config.Ignored.Contains(q)))
        {
            throw new ArgumentException($"{typeof(T).Name} is ignored");
        }
    }

    private static string CreateIndex<T>(MapConfig<T> config, string tableName) where T : class
    {
        var index = config.Index;
        var isUniqueIndex = config.IsUniqueIndex;

        CheckIndex(config);

        if (config.CaseConverter is not null)
        {
            index = index
                .Select(q => config.ParaMaps[q].CustomName ?? config.CaseConverter.Convert(q))
                .ToHashSet();
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
        var mapping = config.ParaMaps
            .Where(q => !config.Ignored.Contains(q.Key))
            .Select(q => q.Value);

        return mapping.Select(map =>
        {
            var name = map.CustomName ??
                config.CaseConverter?.Convert(map.Name) ??
                map.Name;

            var type = GetTypeWithPrecision(map, config);

            var nullability = map.Nullable ? string.Empty : " not null";

            return $"{name} {type}{nullability}";
        });
    }

    // This is really bad, must be refactored
    private static string GetTypeWithPrecision<T>(ParaMap<T> map, MapConfig<T> config) where T : class
    {
        var precisionTemplate = "<precision>";
        var type = map.SqlType ?? _settings.TypeMaps![map.Type.Name.ToLower()];

        var splitted = type.Split(precisionTemplate);
        var precision = () => map.Precision ?? config.DefaultPrecision ?? throw new ArgumentException("no precision is specified");
        var doublePrecision = () => map.DoublePrecision ?? config.DefaultDoublePrecision ?? throw new ArgumentException("no double precision is specified");

        var res = splitted.Length switch
        {
            1 => type,
            2 => splitted[0] + precision.Invoke() + splitted[1],
            3 => splitted[0] + doublePrecision.Invoke().Item1 + splitted[1] + doublePrecision.Invoke().Item2 + splitted[2],
            _ => throw new FormatException("incorrect format")
        };

        return res;
    }
}