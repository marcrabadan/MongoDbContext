using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace MongoDbFramework.Extensions
{
    public static class IndexManagerExtensions
    {
        public static void SetIndices<T>(this IMongoIndexManager<T> indexManager, IList<Tuple<IndexKeysDefinition<T>, CreateIndexOptions<T>>> indices)
        {
            foreach (var index in indices)
            {
                try
                {
                    indexManager.CreateOne(index.Item1, index.Item2);
                }
                catch (Exception)
                {
                    indexManager.DropOne(index.Item2.Name);
                }
            }
        }
    }
}
