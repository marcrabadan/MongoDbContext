using Inflector;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDbFramework.Abstractions;
using System.Linq;

namespace MongoDbFramework
{
    public static class ConfigurationSourceExtensions
    {
        public static MongoClient ToMongoClient<TDocument>(this ConfigurationSource<TDocument> configurationSource) where TDocument : IDocument
        {
            return configurationSource.Source;
        }

        public static IMongoDatabase ToMongoDatabase<TDocument>(this ConfigurationSource<TDocument> configurationSource, MongoClient client) where TDocument : IDocument
        {
            var databaseSettings = configurationSource.Model.DatabaseBehavior.ToMongoDatabaseSettings();
            var databaseName = configurationSource.GetDatabaseName();
            return client.GetDatabase(databaseName, databaseSettings);
        }

        public static MongoDB.Driver.IMongoCollection<TDocument> ToMongoCollection<TDocument>(this ConfigurationSource<TDocument> configurationSource, IMongoDatabase mongoDatabase) where TDocument : IDocument
        {
            MongoDB.Driver.IMongoCollection<TDocument> collection;

            var collectionName = configurationSource.GetCollectionName();
            var collectionSettings = configurationSource.Model.CollectionBehavior.ToMongoCollectionSettings();
            collection = mongoDatabase.GetOrCreateCollection<TDocument>(collectionName, collectionSettings);

            collection.Indexes.SetIndices(configurationSource.Model?.Indices);

            return collection;
        }

        public static string GetCollectionName<TDocument>(this ConfigurationSource<TDocument> configurationSource) where TDocument : IDocument
        {
            return configurationSource.Model != default(Model<TDocument>)
                ? configurationSource.Model.CollectionName
                : typeof(TDocument).Name.ToLower().Pluralize();
        }

        public static string GetDatabaseName<TDocument>(this ConfigurationSource<TDocument> configurationSource) where TDocument : IDocument
        {
            return configurationSource.Model != default(Model<TDocument>)
                ? configurationSource.Model.DatabaseName
                : string.Format("{0}db", typeof(TDocument).Name.ToLower());
        }

        public static GridFSBucket ToGridFsBucket<TFileDocument>(this ConfigurationSource<TFileDocument> configurationSource, IMongoDatabase database) where TFileDocument : IFileDocument
        {
            var options = new GridFSBucketOptions
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

        private static MongoDB.Driver.IMongoCollection<TDocument> GetOrCreateCollection<TDocument>(this IMongoDatabase database, string collectionName, MongoCollectionSettings settings) where TDocument : IDocument
        {
            var filter = new BsonDocument("name", collectionName);
            var exists = database.ListCollectionNames(new ListCollectionNamesOptions {Filter = filter}).FirstOrDefault() != null;
            if (!exists)
                database.CreateCollection(collectionName);
            
            return database.GetCollection<TDocument>(collectionName, settings);
        }
    }
}
