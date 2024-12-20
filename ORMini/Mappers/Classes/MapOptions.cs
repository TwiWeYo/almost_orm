﻿using ORMini.Converters;
using System.Linq.Expressions;
using System.Reflection;

namespace ORMini.Mappers;
public enum OnConflict
{
    Ignore,
    DoNothing,
    Update
}
public class MapOptions<T> : MapOptions where T : class
{
    private static readonly PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    public bool IsUniqueIndex { get; private set; }
    public HashSet<string> Index { get; private set; } = new HashSet<string>();

    public HashSet<string> Ignored { get; private set; } = new HashSet<string>();
    public Dictionary<string, ParaMap<T>> ParaMaps { get; private init; }

    public MapOptions()
    {
        ParaMaps = GetDefaultParaMaps();
    }

    private Dictionary<string, ParaMap<T>> GetDefaultParaMaps()
    {
        var typeExpression = Expression.Parameter(typeof(T), "type");

        return properties
            .Select(property => Expression.MakeMemberAccess(typeExpression, property))
            .Select(member => Expression.Convert(member, typeof(object)))
            .Select(member => Expression.Lambda<Func<T, object>>(member, typeExpression))
            .Select(lambda => ParaMapper.Map(lambda))
            .ToDictionary(q => q.Name);
    }

    #region Override builder

    // In newer version of the language you don't have to do that, however I want .NET 6 support
    public override MapOptions<T> WithTableName(Func<string, string> tableName)
    {
        base.WithTableName(tableName);
        return this;
    }
    public override MapOptions<T> WithTableName(string tableName)
    {
        base.WithTableName(tableName);
        return this;
    }
    public override MapOptions<T> WithProcedureName(Func<string, string> procedureName)
    {
        base.WithProcedureName(procedureName);
        return this;
    }

    public override MapOptions<T> WithProcedureName(string procedureName)
    {
        base.WithProcedureName(procedureName);
        return this;
    }

    public override MapOptions<T> OnConflictDo(OnConflict onConflict)
    {
        base.OnConflictDo(onConflict);
        return this;
    }

    public override MapOptions<T> WithCaseConverter(ICaseConverter caseConverter)
    {
        base.WithCaseConverter(caseConverter);
        return this;
    }

    public override MapOptions<T> WithDefaultPrecision(uint? defaultPrecision)
    {
        base.WithDefaultPrecision(defaultPrecision);
        return this;
    }

    public override MapOptions<T> WithDefaultDoublePrecision((uint, uint)? defaultDoublePrecision)
    {
        base.WithDefaultDoublePrecision(defaultDoublePrecision);
        return this;
    }
    #endregion

    public MapOptions<T> WithIndex(bool isUnique, params string[] indexes)
    {
        IsUniqueIndex = isUnique;
        Index = indexes.ToHashSet();
        return this;
    }

    public MapOptions<T> Ignore(params string[] ignored)
    {
        Ignored = ignored.ToHashSet();
        return this;
    }

    public MapOptions<T> Explicit(params ParaMap<T>[] mapping)
    {
        foreach (var map in mapping)
        {
            ParaMaps[map.Name] = map;
        }

        return this;
    }
}
