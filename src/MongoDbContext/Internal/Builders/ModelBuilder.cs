using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MongoDbFramework.UnitTests")]
[assembly: InternalsVisibleTo("MongoDbFramework.IntegrationTests")]

namespace MongoDbFramework
{
    public class ModelBuilder
    {
        private readonly Dictionary<Type, object> _modelConfig = new Dictionary<Type, object>();
        internal MongoClient Client { get; }

        public ModelBuilder(MongoClient client)
        {
            Client = client;
        }

        public DocumentTypeBuilder<TDocument> Document<TDocument>() where TDocument : IDocument
        {
            return new DocumentTypeBuilder<TDocument>(c => Apply(c));
        }

        internal void Apply<TDocument>(DocumentTypeBuilder<TDocument> modelBuilder) where TDocument : IDocument
        {
            var config = new ConfigurationSource<TDocument>(Client);
            var model = new Model<TDocument>
            {
                DatabaseName = modelBuilder.DatabaseName,
                CollectionName = modelBuilder.CollectionName,
                Indices = modelBuilder.Indexes,
                FileStorageOptions = modelBuilder.FileStorageOptions
            };
            config.Model = model;
            if (!_modelConfig.ContainsKey(typeof(TDocument)))
                _modelConfig.Add(typeof(TDocument), config);
            else
                _modelConfig[typeof(TDocument)] = config;
        }

        internal Dictionary<Type, object> Models => _modelConfig;
    }
}
