using AlmostOrm.Mappers;
using Microsoft.Extensions.Options;
using System.Text;

namespace AlmostOrm;

public class Gennie
{
    private readonly Config _config;

    public Gennie(IOptions<Config> config) : this(config.Value) { }
    public Gennie(Config config)
    {
        _config = config;
    }

    public void MakeTable<T>(string tablePath, MapOptions<T>? options = null) where T : class => MakeTable(_ => tablePath, options);
    public void MakeTable<T>(Func<string, string> tablePath, MapOptions<T>? options = null) where T : class
    {
        options ??= new();
        var name = typeof(T).Name;

        var tableName = options.CaseConverter?.Convert(name) ?? nameof(name);
        tableName = options.TableName?.Invoke(tableName) ?? tableName;

        var index = string.Empty;

        var tableContents = string.Join(",\n\t", GetMappedStrings(options));

        if (options.Index?.Any() == true)
        {
            index = CreateIndex(options, tableName);
        }

        var result = new StringBuilder(_config.Templates["table"])
            .Replace("<table_name>", tableName)
            .Replace("<table_contents>", tableContents)
            .Replace("<index>", index)
            .Replace("\t", "    ");

        WriteToFile(tablePath.Invoke(tableName), result.ToString());
    }

    public void MakeProcedure<T>(string procedurePath, MapOptions<T>? options = null) where T : class => MakeProcedure(_ => procedurePath, options);
    public void MakeProcedure<T>(Func<string, string> procedurePath, MapOptions<T>? options = null) where T : class
    {
        options ??= new();
        var name = typeof(T).Name;

        var procedureName = options.CaseConverter?.Convert(name) ?? nameof(name);
        procedureName = options.ProcedureName?.Invoke(procedureName) ?? procedureName;

        var tableName = options.CaseConverter?.Convert(name) ?? nameof(name);
        tableName = options.TableName?.Invoke(tableName) ?? tableName;

        var index = options.Index;
        var isUniqueIndex = options.IsUniqueIndex;

        CheckIndex(options);

        if (options.CaseConverter is not null)
        {
            index = index?
                .Select(q => options.ParaMaps[q].CustomName ?? options.CaseConverter.Convert(q))
                .ToHashSet();
        }

        var indexContents = index?.Any() == true ? $"({string.Join(", ", index)})" : string.Empty;

        var procedureContents = string.Join(",\n\t", GetMappedStrings(options));

        var procedureData = options.ParaMaps.Values
            .Select
            (
                map =>
                map.CustomName ??
                options.CaseConverter?.Convert(map.Name) ??
                map.Name
            );

        var procedureValues = procedureData
            .Select(q => $"{procedureName}.{q}");

        var onConflict = options.OnConflict switch
        {
            OnConflict.DoNothing => _config.Templates["onConflictDoNothing"]
                .Replace("<index>", indexContents),
            OnConflict.Update => CreateOnConflict(procedureData, indexContents),
            _ => ";"
        };

        var result = new StringBuilder(_config.Templates["procedure"])
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

    private string CreateOnConflict(IEnumerable<string> procedureData, string index)
    {
        var tableContents = string.Join(",\n\t\t", procedureData.Select(q => $"{q} = excluded.{q}"));
        return new StringBuilder(_config.Templates["onConflictUpdate"])
            .Replace("<index>", index)
            .Replace("<table_contents>", tableContents)
            .ToString();
    }

    private static void CheckIndex<T>(MapOptions<T> options) where T : class
    {
        if (options.Index.Any(q => !options.ParaMaps.ContainsKey(q)))
        {
            throw new ArgumentException($"{typeof(T).Name} does not contain such properties");
        }

        if (options.Index.Any(q => options.Ignored.Contains(q)))
        {
            throw new ArgumentException($"{typeof(T).Name} is ignored");
        }
    }

    private string CreateIndex<T>(MapOptions<T> options, string tableName) where T : class
    {
        var index = options.Index;
        var isUniqueIndex = options.IsUniqueIndex;

        CheckIndex(options);

        if (options.CaseConverter is not null)
        {
            index = index
                .Select(q => options.ParaMaps[q].CustomName ?? options.CaseConverter.Convert(q))
                .ToHashSet();
        }

        var indexName = $"{(isUniqueIndex ? "ux_" : "ix_")}{tableName}_{string.Join('_', index)}";
        var indexContents = string.Join(",\n\t", index) + ';';

        return new StringBuilder(_config.Templates["index"])
            .Replace("<unique>", isUniqueIndex ? "unique" : string.Empty)
            .Replace("<index_name>", indexName)
            .Replace("<table_name>", tableName)
            .Replace("<index_contents>", indexContents)
            .ToString();
    }

    private IEnumerable<string> GetMappedStrings<T>(MapOptions<T> options) where T : class
    {
        var mapping = options.ParaMaps
            .Where(q => !options.Ignored.Contains(q.Key))
            .Select(q => q.Value);

        return mapping.Select(map =>
        {
            var name = map.CustomName ??
                options.CaseConverter?.Convert(map.Name) ??
                map.Name;

            var type = GetTypeWithPrecision(map, options);

            var nullability = map.Nullable ? string.Empty : " not null";
            nullability = options.CaseConverter?.Convert(nullability) ?? nullability;

            return $"{name} {type}{nullability}";
        });
    }

    // This is really bad, must be refactored
    private string GetTypeWithPrecision<T>(ParaMap<T> map, MapOptions<T> options) where T : class
    {
        var precisionTemplate = "<precision>";
        var type = map.SqlType ?? _config.TypeMaps![map.Type.Name.ToLower()];

        var splitted = type.Split(precisionTemplate);
        var precision = () => map.Precision ?? options.DefaultPrecision ?? throw new ArgumentException("no precision is specified");
        var doublePrecision = () => map.DoublePrecision ?? options.DefaultDoublePrecision ?? throw new ArgumentException("no double precision is specified");

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