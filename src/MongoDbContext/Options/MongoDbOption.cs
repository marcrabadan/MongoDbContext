using System;
using MongoDB.Driver;

namespace MongoDbFramework
{
    public class MongoDbOption
    {
        public string ConnectionString { get; set; }

        public MongoClientSettings Settings { get; set; }
        
        internal MongoClientSettings Build()
        {
            if (Settings == null)
                throw new InvalidOperationException($"Settings property at ElasticSearchOption is mandatory.");

            return Settings;
        }
    }
}
