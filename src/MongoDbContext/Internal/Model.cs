using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoDbFramework
{
    public sealed class Model<T> : Model where T : IDocument
    {
        public Type DocumentType => typeof(T);
        public FindOptions<T> FindOptions { get; set; }
        public List<Tuple<IndexKeysDefinition<T>, CreateIndexOptions<T>>> Indices { get; set; }
    }

    public class Model
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public FileStorageOptions FileStorageOptions { get; set; }
        public Behavior DatabaseBehavior { get; set; }
        public SessionBehavior SessionBehavior { get; set; }
        public Behavior TransactionBehavior { get; set; }
    }
}
