
using AlmostOrm.Mappers.Classes;

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
    public MapConfig DefaultConfig { get; set; }

    public MapFactory(MapConfig defaultConfig, Func<string, string>? defaultTablePath = null, Func<string, string>? defaultProcedurePath = null)
    {
        DefaultConfig = defaultConfig;
        DefaultTablePath = defaultTablePath ?? (name => $"{name}.sql");
        DefaultProcedurePath = defaultProcedurePath ?? (name => $"{name}_save.sql");
    }
    public MapConfig<T> RegisterMap<T>(MappingTypes mappingTypes = MappingTypes.Table | MappingTypes.Procedure) where T : class
    {
        if (DefaultConfig is null)
        {
            throw new ArgumentNullException($"{nameof(DefaultConfig)} must be specified");
        }

        var res = new MapConfig<T>()
            .WithTableName(DefaultConfig.TableName)
            .WithProcedureName(DefaultConfig.ProcedureName)
            .OnConflictDo(DefaultConfig.OnConflict)
            .WithCaseConverter(DefaultConfig.CaseConverter)
            .WithDefaultPrecision(DefaultConfig.DefaultPrecision)
            .WithDefaultDoublePrecision(DefaultConfig.DefaultDoublePrecision)
            ;

        if (mappingTypes.HasFlag(MappingTypes.Table))
        {
            _tableGens += () => MapGen.MakeTable(DefaultTablePath, res);
        }
        if (mappingTypes.HasFlag(MappingTypes.Procedure))
        {
            _procedureGens += () => MapGen.MakeProcedure(DefaultProcedurePath, res);
        }

        return res;
    }

    public void Compile()
    {
        _tableGens?.Invoke();
        _procedureGens?.Invoke();
    }
}
