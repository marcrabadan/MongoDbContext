using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDbFramework.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDbFramework
{
    public class DocumentTypeBuilder<T> where T : Document
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
