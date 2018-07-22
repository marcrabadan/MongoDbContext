using Inflector;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace MongoDbFramework.Extensions
{
    public static class ConfigurationSourceExtensions
    {
        public static MongoClient ToMongoClient<TDocument>(this ConfigurationSource<TDocument> configurationSource) where TDocument : Document
        {
            return configurationSource.Source;
        }

        public static IMongoDatabase ToMongoDatabase<TDocument>(this ConfigurationSource<TDocument> configurationSource, MongoClient client) where TDocument : Document
        {
            var database = default(IMongoDatabase);
            if (configurationSource.Model != default(Model<TDocument>))
                database = client.GetDatabase(configurationSource.Model.DatabaseName);
            else
            {
                var dbName = string.Format("{0}db", typeof(TDocument).Name.ToLower());
                database = client.GetDatabase(dbName);
            }

            return database;
        }

        public static MongoDB.Driver.IMongoCollection<TDocument> ToMongoCollection<TDocument>(this ConfigurationSource<TDocument> configurationSource, MongoClient client) where TDocument : Document
        {
            var collection = default(MongoDB.Driver.IMongoCollection<TDocument>);
            if (configurationSource.Model != default(Model<TDocument>))
            {
                collection = client.GetDatabase(configurationSource.Model.DatabaseName)
                    .GetCollection<TDocument>(configurationSource.Model.CollectionName);

                collection.Indexes.SetIndices(configurationSource.Model?.Indices);
            }
            else
            {
                var dbName = string.Format("{0}db", typeof(TDocument).Name.ToLower());
                collection = client.GetDatabase(dbName)
                    .GetCollection<TDocument>(typeof(TDocument).Name.ToLower().Pluralize());
            }

            return collection;
        }

        public static GridFSBucket ToGridFsBucket<TFileDocument>(this ConfigurationSource<TFileDocument> configurationSource, IMongoDatabase database) where TFileDocument : Document
        {
            var options = new GridFSBucketOptions()
            {
                BucketName = nameof(TFileDocument),
                ChunkSizeBytes = 1048576
            };

            if (configurationSource?.Model?.FileStorageOptions != null)
            {
                var fileStorageOptions = configurationSource.Model.FileStorageOptions;
                options.BucketName = fileStorageOptions.BucketName;
                options.ChunkSizeBytes = fileStorageOptions.ChunkSize;
                options.ReadPreference = fileStorageOptions.ReadPreference;
                options.ReadConcern = fileStorageOptions.ReadConcern;
                options.WriteConcern = fileStorageOptions.WriteConcern;
            }

            return new GridFSBucket(database, options);
        }
    }
}
