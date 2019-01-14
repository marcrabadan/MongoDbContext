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
                    var options = index.Item2;
                    options.Background = true;
                    options.Sparse = true;
                    
                    var indexModel = new CreateIndexModel<TDocument>(index.Item1, options);
                    
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
