﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MongoDbContext.Documents;
using MongoDbContext.Internal;
using MongoDbContext.Internal.Builders;
using MongoDB.Driver;

[assembly: InternalsVisibleTo("MongoDbContext.UnitTests")]

namespace MongoDbContext
{
    public class ModelBuilder
    {
        private readonly Dictionary<Type, object> _modelConfig = new Dictionary<Type, object>();
        internal MongoClient Client { get; }

        public ModelBuilder(MongoClient client)
        {
            Client = client;
        }

        public DocumentTypeBuilder<TDocument> Document<TDocument>() where TDocument : Document
        {
            var documentTypeBuilder = new DocumentTypeBuilder<TDocument>(c => Apply(c));
            return documentTypeBuilder;
        }

        internal void Apply<TDocument>(DocumentTypeBuilder<TDocument> modelBuilder) where TDocument : Document
        {
            var config = new ConfigurationSource<TDocument>(Client);
            var model = new Model<TDocument>
            {
                DatabaseName = modelBuilder.DatabaseName,
                CollectionName = modelBuilder.CollectionName
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
