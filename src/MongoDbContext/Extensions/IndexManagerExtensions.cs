using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace MongoDbFramework
{
    public static class IndexManagerExtensions
    {
        public static void SetIndices<TDocument>(this IMongoIndexManager<TDocument> indexManager, IList<Tuple<IndexKeysDefinition<TDocument>, CreateIndexOptions<TDocument>>> indices) where TDocument : IDocument
        {
            foreach (var index in indices)
            {
                try
                {
                    var indexModel = new CreateIndexModel<TDocument>(index.Item1, index.Item2);
                    
                    indexManager.CreateOne(indexModel);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
