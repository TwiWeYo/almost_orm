using System.Linq.Expressions;

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
        public string TableName { get; private set; }
        public bool HasId { get; private set; }
        public OnConflict OnConflict { get; private set; }
        public bool IsUniqueIndex { get; private set; }
        public string[] Index { get; private set; }

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
    }
}
