﻿using MongoDB.Driver;
using System;
using System.Linq;
using MongoDbFramework.Documents;

namespace MongoDbFramework
{
    public class MongoDbContext 
    {        
        private readonly MongoClient _mongoClient;

        public MongoDbContext(MongoDbOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("The options argument at MongoDbContext is mandatory.");
            
            _mongoClient = options.MongoClient;
            DiscoverAndInitializeCollections();
        }

        internal void DiscoverAndInitializeCollections()
        {
            var discoveryProperties = new PropertyDiscovery<MongoDbContext>(this);
            discoveryProperties.Initialize(typeof(MongoCollection<>), typeof(IMongoCollection<>), "Collection");
            discoveryProperties.Initialize(typeof(MongoFileCollection<>), typeof(IMongoFileCollection<>), "FileCollection");
        }

        public virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
           
        public MongoCollection<TDocument> Collection<TDocument>() where TDocument : Document
        {
            var modelBuilder = new ModelBuilder(_mongoClient);
            OnModelCreating(modelBuilder);
            var configurationSource = modelBuilder.Models.Any()
                ? (ConfigurationSource<TDocument>) modelBuilder.Models[typeof(TDocument)]
                : new ConfigurationSource<TDocument>(_mongoClient);
            return new MongoCollection<TDocument>(configurationSource);
        }

        public MongoFileCollection<TDocument> FileCollection<TDocument>() where TDocument : FileDocument, new()
        {
            var modelBuilder = new ModelBuilder(_mongoClient);
            OnModelCreating(modelBuilder);
            var configurationSource = modelBuilder.Models.Any()
                ? (ConfigurationSource<TDocument>)modelBuilder.Models[typeof(TDocument)]
                : new ConfigurationSource<TDocument>(_mongoClient);
            return new MongoFileCollection<TDocument>(configurationSource);
        }
    }
}