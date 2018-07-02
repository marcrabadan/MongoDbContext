using System;
using System.Security.Authentication;
using MongoDbContext.Internal.Constants;
using MongoDB.Driver;

namespace MongoDbContext.Options.Builders
{
    public class MongoDbOptionBuilder
    {
        private readonly MongoDbOptions _mongoDbOptions;
        private readonly MongoDbOption _mongoDbOption;
        private MongoClientSettings _mongoClientSettings;

        public MongoDbOptionBuilder(MongoDbOptions options)
        {
            _mongoDbOptions = options;
            _mongoDbOption = new MongoDbOption();
            _mongoClientSettings = new MongoClientSettings();
        }

        public void ConnectionString(string connectionString)
        {
            if (connectionString.Contains(AzureConstants.AzureCosmosDbDomain))
            {
                _mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                _mongoClientSettings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                _mongoDbOption.Settings = _mongoClientSettings;
            }
            else
                _mongoDbOption.ConnectionString = connectionString;
        }

        public void Configure(Action<MongoClientSettings> builder)
        {
            builder.Invoke(_mongoClientSettings);
            _mongoDbOption.Settings = _mongoClientSettings;
        }

        public virtual MongoDbOptions Options
        {
            get
            {
                _mongoDbOptions.Options = _mongoDbOption;
                return _mongoDbOptions;
            }
        }
    }

    public class MongoDbOptionBuilder<TContext> : MongoDbOptionBuilder where TContext : MongoDbContext
    {
        public MongoDbOptionBuilder() : base(new MongoDbOptions<TContext>())
        {
        }
    }
}
