
using AlmostOrm.Mappers;

namespace AlmostOrm;

[Flags]
public enum MappingTypes
{
    Table,
    Procedure
}

public class MapFactory
{
    private Action? _tableGens;
    private Action? _procedureGens;

    public Func<string, string> DefaultTablePath { get; init; }
    public Func<string, string> DefaultProcedurePath { get; init; }
    public MapOptions DefaultOptions { get; set; }

    public MapFactory(MapOptions defaultOptions, Func<string, string>? defaultTablePath = null, Func<string, string>? defaultProcedurePath = null)
    {
        DefaultOptions = defaultOptions;
        DefaultTablePath = defaultTablePath ?? (name => $"{name}.sql");
        DefaultProcedurePath = defaultProcedurePath ?? (name => $"{name}_save.sql");
    }
    public MapOptions<T> RegisterMap<T>(MappingTypes mappingTypes = MappingTypes.Table | MappingTypes.Procedure) where T : class
    {
        if (DefaultOptions is null)
        {
            throw new ArgumentNullException($"{nameof(DefaultOptions)} must be specified");
        }

        var res = new MapOptions<T>()
            .WithTableName(DefaultOptions.TableName)
            .WithProcedureName(DefaultOptions.ProcedureName)
            .OnConflictDo(DefaultOptions.OnConflict)
            .WithCaseConverter(DefaultOptions.CaseConverter)
            .WithDefaultPrecision(DefaultOptions.DefaultPrecision)
            .WithDefaultDoublePrecision(DefaultOptions.DefaultDoublePrecision)
            ;

        if (mappingTypes.HasFlag(MappingTypes.Table))
        {
            _tableGens += () => GenOrm.MakeTable(DefaultTablePath, res);
        }
        if (mappingTypes.HasFlag(MappingTypes.Procedure))
        {
            _procedureGens += () => GenOrm.MakeProcedure(DefaultProcedurePath, res);
        }

        return res;
    }

    public void Compile()
    {
        _tableGens?.Invoke();
        _procedureGens?.Invoke();
    }
}
