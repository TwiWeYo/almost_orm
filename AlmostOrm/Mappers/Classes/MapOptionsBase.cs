using ORMini.Converters;

namespace ORMini.Mappers;

public class MapOptions
{
    public Func<string, string>? TableName { get; private set; }
    public Func<string, string>? ProcedureName { get; private set; }

    public uint? DefaultPrecision { get; private set; }
    public (uint, uint)? DefaultDoublePrecision { get; private set; }
    public OnConflict OnConflict { get; private set; } = OnConflict.Ignore;
    public ICaseConverter? CaseConverter { get; private set; }

    public virtual MapOptions WithTableName(Func<string, string> tableDelegate)
    {
        TableName = tableDelegate;
        return this;
    }
    public virtual MapOptions WithTableName(string tableName) => WithTableName(_ => tableName);
    public virtual MapOptions WithProcedureName(Func<string, string> procedureDelegate)
    {
        ProcedureName = procedureDelegate;
        return this;
    }
    public virtual MapOptions WithProcedureName(string procedureName) => WithProcedureName(_ => procedureName);

    public virtual MapOptions OnConflictDo(OnConflict onConflict)
    {
        OnConflict = onConflict;
        return this;
    }

    public virtual MapOptions WithCaseConverter(ICaseConverter caseConverter)
    {
        CaseConverter = caseConverter;
        return this;
    }

    public virtual MapOptions WithDefaultPrecision(uint? defaultPrecision)
    {
        DefaultPrecision = defaultPrecision;
        return this;
    }

    public virtual MapOptions WithDefaultDoublePrecision((uint, uint)? defaultDoublePrecision)
    {
        DefaultDoublePrecision = defaultDoublePrecision;
        return this;
    }
}
