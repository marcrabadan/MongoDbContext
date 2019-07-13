using MongoDbFramework.Abstractions;
using System;

namespace MongoDbFramework
{
    public interface IIndexer<T> where T : IDocument
    {
        object Ascending(Func<T, object> builder);
        object Descending(Func<T, object> builder);
    }
}
