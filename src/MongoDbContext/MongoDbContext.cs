using MongoDB.Driver;
using MongoDbFramework.Abstractions;
using System;
using System.Linq;

namespace MongoDbFramework
{
    public class MongoDbContext 
    {        
        public MongoDbContext(MongoDbOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("The options argument at MongoDbContext is mandatory.");

            MongoClient = options.MongoClient;
            DiscoverAndInitializeCollections();
        }

        internal MongoClient MongoClient { get; }

        internal void DiscoverAndInitializeCollections()
        {
            var discoveryProperties = new PropertyDiscovery<MongoDbContext>(this);
            discoveryProperties.Initialize(typeof(MongoCollection<>), typeof(IMongoCollection<>), "Collection");
            discoveryProperties.Initialize(typeof(MongoFileCollection<>), typeof(IMongoFileCollection<>), "FileCollection");
        }

        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
           
        public MongoCollection<TDocument> Collection<TDocument>() where TDocument : IDocument
        {
            var modelBuilder = new ModelBuilder(MongoClient);
            OnModelCreating(modelBuilder);
            var configurationSource = modelBuilder.Models.Any()
                ? (ConfigurationSource<TDocument>) modelBuilder.Models[typeof(TDocument)]
                : new ConfigurationSource<TDocument>(MongoClient);
            return new MongoCollection<TDocument>(configurationSource);
        }

        public MongoFileCollection<TDocument> FileCollection<TDocument>() where TDocument : IFileDocument, new()
        {
            var modelBuilder = new ModelBuilder(MongoClient);
            OnModelCreating(modelBuilder);
            var configurationSource = modelBuilder.Models.Any()
                ? (ConfigurationSource<TDocument>)modelBuilder.Models[typeof(TDocument)]
                : new ConfigurationSource<TDocument>(MongoClient);
            return new MongoFileCollection<TDocument>(configurationSource);
        }
    }
}
