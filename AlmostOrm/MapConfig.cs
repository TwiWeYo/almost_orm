﻿using AlmostOrm.Converters;
using System.Linq.Expressions;
using System.Reflection;

namespace AlmostOrm.MapConfig
{
    public enum OnConflict
    {
        Ignore,
        DoNothing,
        Update
    }
    public class MapConfig<T> where T : class
    {
        private static readonly PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        public Func<string, string>? TableName { get; private set; }
        public Func<string, string>? ProcedureName { get; private set; }
        public bool HasId { get; private set; }
        public OnConflict OnConflict { get; private set; } = OnConflict.Ignore;
        public bool IsUniqueIndex { get; private set; }
        public HashSet<string> Index { get; private set; } = new HashSet<string>();
        public HashSet<string> Ignored { get; private set; } = new HashSet<string>();
        public ICaseConverter? CaseConverter { get; private set; }
        public Dictionary<string, ParaMap<T>> ParaMaps { get; private init; }

        public MapConfig()
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

        public MapConfig<T> WithTableName(Func<string, string> tableName)
        {
            TableName = tableName;
            return this;
        }
        public MapConfig<T> WithProcedureName(Func<string, string> procedureName)
        {
            ProcedureName = procedureName;
            return this;
        }

        public MapConfig<T> WithId()
        {
            HasId = true;
            return this;
        }

        public MapConfig<T> OnConflictDo(OnConflict onConflict)
        {
            OnConflict = onConflict;
            return this;
        }

        public MapConfig<T> WithIndex(bool isUnique, params string[] indexes)
        {
            IsUniqueIndex = isUnique;
            Index = indexes.ToHashSet();
            return this;
        }

        public MapConfig<T> Ignore(params string[] ignored)
        {
            Ignored = ignored.ToHashSet();
            return this;
        }

        public MapConfig<T> WithCaseConverter(ICaseConverter caseConverter)
        {
            CaseConverter = caseConverter;
            return this;
        }

        public MapConfig<T> Explicit(params ParaMap<T>[] mapping)
        {
            foreach (var map in mapping)
            {
                ParaMaps[map.Name] = map;
            }

            return this;
        }
    }
}
