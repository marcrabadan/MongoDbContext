using System;

namespace MongoDbFramework
{
    public interface IIndexer<T> where T : Document
    {
        object Ascending(Func<T, object> builder);
        object Descending(Func<T, object> builder);
    }
}
