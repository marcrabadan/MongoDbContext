using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDbFramework.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDbFramework
{
    public sealed class DocumentTypeBuilder<T> where T : IDocument
    {
        private Action<DocumentTypeBuilder<T>> _apply;
        private bool _isDefaultDatabaseBehavior = true;
        private bool _isDefaultCollectionBehavior = true;
        private bool _isDefaultSessionBehavior = true;
        private bool _isDefaultTransactionBehavior = true;

        public DocumentTypeBuilder(Action<DocumentTypeBuilder<T>> apply)
        {
            _apply = apply;
            IsFileDocument = false;
            Indexes = new List<Tuple<IndexKeysDefinition<T>, CreateIndexOptions<T>>>();
            ApplyDefaultBehavior();
        }

        internal string DatabaseName { get; set; }
        internal string CollectionName { get; set; }
        internal List<Tuple<IndexKeysDefinition<T>, CreateIndexOptions<T>>> Indexes { get; set; }
        internal bool IsFileDocument { get; set; }
        internal FileStorageOptions FileStorageOptions { get; set; }
        internal FindOptions<T> FindOptions { get; set; }
        internal Behavior DatabaseBehavior { get; set; }
        internal Behavior CollectionBehavior { get; set; }
        internal SessionBehavior SessionBehavior { get; set; }
        internal Behavior TransactionBehavior { get; set; }
        
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

        /// <summary>
        /// By default, find options has not cursor timeout and batch size is set to 1000
        /// </summary>
        /// <param name="findOptions">Configure a custom find options for this collection</param>
        /// <returns></returns>
        public DocumentTypeBuilder<T> DefineFindOptions(Action<FindOptions<T>> findOptions)
        {
            this.FindOptions = new FindOptions<T>
            {
                BatchSize = 1000,
                NoCursorTimeout = false
            };
            findOptions.Invoke(this.FindOptions);
            _apply(this);
            return this;
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

        public FileDocumentTypeBuilder<TFileDocument> AsFileStorage<TFileDocument>() where TFileDocument : IFileDocument
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
        
        public DocumentTypeBuilder<T> WithDatabaseBehavior(Action<BehaviorBuilder<T>> behavior)
        {
            var builder = new BehaviorBuilder<T>(ApplyDatabaseBehavior);
            behavior.Invoke(builder);
            DatabaseBehavior = builder.Build();
            _isDefaultDatabaseBehavior = false;
            _apply(this);
            return this;
        }
        public DocumentTypeBuilder<T> WithCollectionBehavior(Action<BehaviorBuilder<T>> behavior)
        {
            var builder = new BehaviorBuilder<T>(ApplyCollectionBehavior);
            behavior.Invoke(builder);
            CollectionBehavior = builder.Build();
            _isDefaultCollectionBehavior = false;
            _apply(this);
            return this;
        }

        public DocumentTypeBuilder<T> WithSessionBehavior(Action<SessionBehaviorBuilder<T>> behavior)
        {
            var builder = new SessionBehaviorBuilder<T>(ApplySessionBehavior);            
            behavior.Invoke(builder);
            SessionBehavior = builder.Build();
            _isDefaultSessionBehavior = false;
            _apply(this);
            return this;
        }

        public DocumentTypeBuilder<T> WithTransactionBehavior(Action<BehaviorBuilder<T>> behavior)
        {
            var builder = new BehaviorBuilder<T>(c => ApplyTransactionBehavior(c));
            behavior.Invoke(builder);
            TransactionBehavior = builder.Build();
            _isDefaultTransactionBehavior = false;
            _apply(this);
            return this;
        }

        internal void ApplyDatabaseBehavior(BehaviorBuilder<T> builder)
        {
            var behavior = builder.Build();
            DatabaseBehavior = behavior;
            _apply(this);
        }

        internal void ApplyCollectionBehavior(BehaviorBuilder<T> builder)
        {
            var behavior = builder.Build();
            CollectionBehavior = behavior;
            _apply(this);
        }

        internal void ApplySessionBehavior(SessionBehaviorBuilder<T> builder)
        {
            var sessionBehavior = builder.Build();
            SessionBehavior = sessionBehavior;
            _apply(this);
        }

        internal void ApplyTransactionBehavior(BehaviorBuilder<T> builder)
        {
            var behavior = builder.Build();
            TransactionBehavior = behavior;
            _apply(this);
        }

        internal void Apply<TFileDocument>(FileDocumentTypeBuilder<TFileDocument> modelBuilder) where TFileDocument : IFileDocument
        {
            IsFileDocument = true;
            FileStorageOptions = modelBuilder.Build();
            _apply(this);
        }

        internal void ApplyDefaultBehavior()
        {
            if(_isDefaultDatabaseBehavior)
            {
                DatabaseBehavior = new Behavior
                {
                    ReadPreference = ReadPreference.Secondary,
                    ReadConcern = ReadConcern.Linearizable,
                    WriteConcern = WriteConcern.WMajority
                };
            }
            if (_isDefaultCollectionBehavior)
            {
                CollectionBehavior = new Behavior
                {
                    ReadPreference = ReadPreference.Secondary,
                    ReadConcern = ReadConcern.Linearizable,
                    WriteConcern = WriteConcern.WMajority
                };
            }
            if (_isDefaultSessionBehavior)
            {
                SessionBehavior = new SessionBehavior
                {
                    CasualConsistency = true,                    
                    ReadPreference = ReadPreference.Primary,
                    ReadConcern = ReadConcern.Linearizable,
                    WriteConcern = WriteConcern.WMajority
                };
            }
            if (_isDefaultTransactionBehavior)
            {
                TransactionBehavior = new Behavior
                {
                    ReadPreference = ReadPreference.Primary,
                    ReadConcern = ReadConcern.Linearizable,
                    WriteConcern = WriteConcern.WMajority
                };
            }
            
            _apply(this);
        }
    }
}
