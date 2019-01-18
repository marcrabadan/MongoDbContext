using System;
using MongoDB.Driver;

namespace MongoDbFramework
{
    public class MongoDbOption
    {
        public MongoClientSettings Settings { get; set; }
        
        internal MongoClientSettings Build()
        {
            if (Settings == null)
                throw new InvalidOperationException($"Settings property at MongoDbOption is mandatory.");

            return Settings;
        }
    }
}
