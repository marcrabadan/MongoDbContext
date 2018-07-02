using System;
using MongoDbContext.Documents;

namespace MongoDbContext.Internal
{
    public class Model<T> : Model where T : Document
    {
        public Type DocumentType => typeof(T);
    }

    public class Model
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
}
