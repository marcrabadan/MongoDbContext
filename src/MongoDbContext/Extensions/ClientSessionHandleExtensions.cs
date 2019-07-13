using MongoDB.Driver;
using MongoDbFramework.Abstractions;

namespace MongoDbFramework.Extensions
{
    public static class ClientSessionHandleExtensions
    {
        public static MongoDB.Driver.IMongoCollection<TDocument> ToCollection<TDocument>(this IClientSessionHandle session, ConfigurationSource<TDocument> configurationSource) where TDocument : IDocument
        {
            var databaseSettings = configurationSource.Model.DatabaseBehavior.ToMongoDatabaseSettings();
            var collectionSettings = configurationSource.Model.CollectionBehavior.ToMongoCollectionSettings();
            var dbName = configurationSource.GetDatabaseName();
            var collectionName = configurationSource.GetCollectionName();
            var collection = session.Client.GetDatabase(dbName, databaseSettings).GetCollection<TDocument>(collectionName, collectionSettings);
            collection.Indexes.SetIndices(configurationSource.Model?.Indices);
            return collection;
        }
    }
}
