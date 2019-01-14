using System;
using MongoDB.Driver;

namespace MongoDbFramework
{
    public class MongoDbOptions<TContext> : MongoDbOptions where TContext : MongoDbContext
    {
        public override Type ContextType => typeof(TContext);
    }

    public abstract class MongoDbOptions
    {
        internal MongoClient _mongoClient;

        public MongoDbOption Options { get; set; }

        internal MongoClient MongoClient
        {
            get
            {
                ValidateOptions();

                if (_mongoClient == null)
                    _mongoClient = new MongoClient(Options.Settings);
                  
                return _mongoClient;
            }
            set
            {
                ValidateOptions();
                _mongoClient = value;
            }
        }

        public abstract Type ContextType { get; }
        
        private void ValidateOptions()
        {
            if (Options.Settings == default(MongoClientSettings))
                throw new InvalidOperationException("Invalid MongoDbOptions configuration for this context, at least one to have that informed.");
        }
    }
}
