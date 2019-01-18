using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Inflector;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.GridFS;

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
            IMongoDatabase database;
            var databaseSettings = configurationSource.Model.DatabaseBehavior.ToMongoDatabaseSettings();
            
            if (configurationSource.Model != default(Model<TDocument>))
            {
                database = client.GetDatabase(configurationSource.Model.DatabaseName, databaseSettings);
            }
            else
            {
                var dbName = string.Format("{0}db", typeof(TDocument).Name.ToLower());
                database = client.GetDatabase(dbName, databaseSettings);
            }
            
            return database;
        }

        public static MongoDB.Driver.IMongoCollection<TDocument> ToMongoCollection<TDocument>(this ConfigurationSource<TDocument> configurationSource, IMongoDatabase mongoDatabase) where TDocument : IDocument
        {
            MongoDB.Driver.IMongoCollection<TDocument> collection;

            var collectionName = configurationSource.Model != default(Model<TDocument>)
                ? configurationSource.Model.CollectionName
                : typeof(TDocument).Name.ToLower().Pluralize();

            collection = mongoDatabase.GetOrCreateCollection<TDocument>(collectionName);
            
            collection.Indexes.SetIndices(configurationSource.Model?.Indices);

            return collection;
        }

        public static GridFSBucket ToGridFsBucket<TFileDocument>(this ConfigurationSource<TFileDocument> configurationSource, IMongoDatabase database) where TFileDocument : FileDocument
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

        private static MongoDB.Driver.IMongoCollection<TDocument> GetOrCreateCollection<TDocument>(this IMongoDatabase database, string collectionName) where TDocument : IDocument
        {
            var filter = new BsonDocument("name", collectionName);
            var exists = database.ListCollectionNames(new ListCollectionNamesOptions {Filter = filter}).FirstOrDefault() != null;
            if (!exists)
                database.CreateCollection(collectionName);
            
            return database.GetCollection<TDocument>(collectionName);
        }
    }
}
