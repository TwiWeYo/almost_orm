namespace AlmostOrm.Mappers;

public sealed class ParaMap<T> where T : class
{

    internal string Name { get; init; }
    internal Type Type { get; init; }
    public string? CustomName { get; private set; }
    public string? SqlType { get; private set; }
    public uint? Precision { get; private set; }
    public (uint, uint)? DoublePrecision { get; private set; }
    public bool Nullable { get; private set; }

    public ParaMap(string name, Type type, bool isNullable)
    {
        Name = name;
        Type = type;
        Nullable = isNullable;
    }
    public ParaMap<T> WithCustomName(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        CustomName = name;
        return this;
    }

    public ParaMap<T> WithType(string type)
    {
        if (string.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));

        SqlType = type;
        return this;
    }

    public ParaMap<T> WithPrecision(uint? precision)
    {
        Precision = precision;
        return this;
    }

    public ParaMap<T> WithDoublePrecision(uint precision, uint doublePrecision)
    {
        DoublePrecision = (precision, doublePrecision);
        return this;
    }

    public ParaMap<T> WithDoublePrecision((uint, uint)? precision)
    {
        DoublePrecision = precision;
        return this;
    }

    public ParaMap<T> IsNullable(bool isNullable)
    {
        Nullable = isNullable;
        return this;
    }
}
