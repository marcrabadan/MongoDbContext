using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoDbFramework
{
    public class Model<T> : Model where T : Document
    {
        public Type DocumentType => typeof(T);
        public List<Tuple<IndexKeysDefinition<T>, CreateIndexOptions<T>>> Indices { get; set; }
    }

    public class Model
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
}
