using System.Linq.Expressions;
using System.Reflection;

namespace AlmostOrm.MapConfig
{
    public enum OnConflict
    {
        Ignore,
        Nothing,
        Update
    }
    public class MapConfig<T> where T : class
    {
        private static readonly PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        public string TableName { get; private set; }
        public bool HasId { get; private set; }
        public OnConflict OnConflict { get; private set; }
        public bool IsUniqueIndex { get; private set; }
        public string[] Index { get; private set; }
        public Dictionary<string, ParaMap<T>> Mapping { get; private init; }

        public MapConfig()
        {
            Mapping = GetDefaultParaMaps();
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

        public MapConfig<T> WithTableName(string tableName)
        {
            TableName = tableName;
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

        public MapConfig<T> WithIndex(bool isUnique, params string[] selector)
        {
            IsUniqueIndex = isUnique;
            Index = selector;
            return this;
        }

        public MapConfig<T> Explicit(params ParaMap<T>[] mapping)
        {
            foreach (var map in mapping)
            {
                Mapping[map.Name] = map;
            }

            return this;
        }
    }
}
