using AlmostOrm.Converters;

namespace AlmostOrm.Mappers;

public class MapConfig
{
    public Func<string, string>? TableName { get; private set; }
    public Func<string, string>? ProcedureName { get; private set; }

    public uint? DefaultPrecision { get; private set; }
    public (uint, uint)? DefaultDoublePrecision { get; private set; }
    public OnConflict OnConflict { get; private set; } = OnConflict.Ignore;
    public ICaseConverter? CaseConverter { get; private set; }

    public virtual MapConfig WithTableName(Func<string, string> tableDelegate)
    {
        TableName = tableDelegate;
        return this;
    }
    public virtual MapConfig WithTableName(string tableName) => WithTableName(_ => tableName);
    public virtual MapConfig WithProcedureName(Func<string, string> procedureDelegate)
    {
        ProcedureName = procedureDelegate;
        return this;
    }
    public virtual MapConfig WithProcedureName(string procedureName) => WithProcedureName(_ => procedureName);

    public virtual MapConfig OnConflictDo(OnConflict onConflict)
    {
        OnConflict = onConflict;
        return this;
    }

    public virtual MapConfig WithCaseConverter(ICaseConverter caseConverter)
    {
        CaseConverter = caseConverter;
        return this;
    }

    public virtual MapConfig WithDefaultPrecision(uint? defaultPrecision)
    {
        DefaultPrecision = defaultPrecision;
        return this;
    }

    public virtual MapConfig WithDefaultDoublePrecision((uint, uint)? defaultDoublePrecision)
    {
        DefaultDoublePrecision = defaultDoublePrecision;
        return this;
    }
}
