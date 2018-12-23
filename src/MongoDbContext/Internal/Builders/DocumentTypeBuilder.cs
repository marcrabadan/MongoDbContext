﻿using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDbFramework
{
    public class DocumentTypeBuilder<T> where T : IDocument
    {
        private Action<DocumentTypeBuilder<T>> _apply;

        public DocumentTypeBuilder(Action<DocumentTypeBuilder<T>> apply)
        {
            _apply = apply;
            IsFileDocument = false;
            Indexes = new List<Tuple<IndexKeysDefinition<T>, CreateIndexOptions<T>>>();
        }

        internal string DatabaseName { get; set; }
        internal string CollectionName { get; set; }
        internal List<Tuple<IndexKeysDefinition<T>, CreateIndexOptions<T>>> Indexes { get; set; }
        internal bool IsFileDocument { get; set; }
        internal FileStorageOptions FileStorageOptions { get; set; }
        
        public DocumentTypeBuilder<T> Map(Action<BsonClassMap<T>> bsonClassMapAction)
        {
            try
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                {
                    BsonClassMap.RegisterClassMap<T>(bsonClassMapAction);
                }
                return this;
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("An item with the same key has already been added"))
                {
                    return this;
                }
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DocumentTypeBuilder<T> WithDatabase(string name)
        {
            DatabaseName = name;
            _apply(this);
            return this;
        }

        public DocumentTypeBuilder<T> WithCollection(string name)
        {
            CollectionName = name;
            _apply(this);
            return this;
        }

        public FileDocumentTypeBuilder<TFileDocument> AsFileStorage<TFileDocument>() where TFileDocument : FileDocument
        {
            var builder = new FileDocumentTypeBuilder<TFileDocument>(c => Apply(c));
            _apply(this);
            return builder;
        }

        public DocumentTypeBuilder<T> DefineIndex(Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> builder, Action<CreateIndexOptions<T>> options)
        {
            IndexKeysDefinitionBuilder<T> indexBuilder = Builders<T>.IndexKeys;
            var indexOptionsBuilder = new CreateIndexOptions<T>();
            var definition = builder(indexBuilder);
            options(indexOptionsBuilder);
            if(Indexes.Any(c => c.Item2.Name == indexOptionsBuilder.Name))
                throw new ArgumentException($"this '{indexOptionsBuilder.Name}' index name exists.");
            Indexes.Add(Tuple.Create(definition, indexOptionsBuilder));
            _apply(this);
            return this;
        }

        internal void Apply<TFileDocument>(FileDocumentTypeBuilder<TFileDocument> modelBuilder) where TFileDocument : FileDocument
        {
            IsFileDocument = true;
            FileStorageOptions = modelBuilder.Build();
            _apply(this);
        }
    }
}
