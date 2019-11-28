using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using MongoDbFramework.Abstractions;
using MongoDbFramework.Extensions;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{
    public class MongoCollection<TDocument> : IMongoCollection<TDocument> where TDocument : IDocument
    {
        private IClientSessionHandle clientSessionHandle;
        private readonly ConfigurationSource<TDocument> configurationSource;
        private readonly RetryPolicy transactionRetryPolicy;
        private readonly RetryPolicy commitRetryPolicy;

        public MongoCollection(ConfigurationSource<TDocument> configurationSource)
        {
            this.configurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));

            Client = this.configurationSource?.ToMongoClient();
            Database = this.configurationSource?.ToMongoDatabase(Client);
            Collection = this.configurationSource?.ToMongoCollection(Database);
                            
            transactionRetryPolicy = Policy
                .Handle<MongoException>(c => c.HasErrorLabel("TransientTransactionError"))
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            commitRetryPolicy = Policy
                .Handle<MongoException>(c => c.HasErrorLabel("UnknownTransactionCommitResult"))                
                .WaitAndRetryAsync(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );
        }
        
        protected internal MongoClient Client { get; set; }
        protected internal IMongoDatabase Database { get; set; }
        protected internal MongoDB.Driver.IMongoCollection<TDocument> Collection { get; set; }
        protected internal GridFSBucket Bucket { get; set; }

        public async Task<IClientSessionHandle> BeginSessionAsync(Action<SessionBehavior> sessionBehaviorAction = default(Action<SessionBehavior>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var sessionBehavior = new SessionBehavior();
            sessionBehaviorAction?.Invoke(sessionBehavior);
            sessionBehavior = sessionBehaviorAction == null ? this.configurationSource.Model.SessionBehavior : default(SessionBehavior);
                       
            var task = this.configurationSource.Source.StartSessionAsync(sessionBehavior.ToClientSessionOptions(), cancellationToken);
#if NETFULL
            this.clientSessionHandle = await task.ConfigureAwait(false);
#else
            this.clientSessionHandle = await task;
#endif
            return this.clientSessionHandle;
        }
        
        public async Task DoTransactionAsync(Func<CancellationToken, Task> txnAction, Action<Behavior> transactionBehaviorAction = default(Action<Behavior>), Action<SessionBehavior> sessionBehaviorAction = default(Action<SessionBehavior>), IClientSessionHandle parentSession = default(IClientSessionHandle), CancellationToken cancellationToken = default(CancellationToken))
        {
            var sessionBehavior = new SessionBehavior();
            sessionBehaviorAction?.Invoke(sessionBehavior);
            sessionBehavior = sessionBehaviorAction == null ? this.configurationSource.Model.SessionBehavior : default(SessionBehavior);
            using (var session = await this.configurationSource.Source.StartSessionAsync(sessionBehavior.ToClientSessionOptions(), cancellationToken).ConfigureAwait(false))
            {                
                try
                {
                    this.clientSessionHandle = session;
                    var task = transactionRetryPolicy.ExecuteAsync((cToken) => RunTransactionAsync(txnAction, session, parentSession, transactionBehaviorAction, cToken), cancellationToken);
#if NETFULL
                    await task.ConfigureAwait(false);
#else
                    await task;
#endif
                }
                catch (Exception)
                {
                    this.clientSessionHandle = default(IClientSessionHandle);
                    throw;
                }
            }
            this.clientSessionHandle = default(IClientSessionHandle);
        }
        
        public Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            IFindFluent<TDocument, TDocument> findFluent;
            if (IsInTransaction())
                findFluent = Collection.Find(this.clientSessionHandle, expression);
            else
                findFluent = Collection.Find(expression);

            return findFluent.FirstOrDefaultAsync(cancellationToken);
        }

        public Task<List<TDocument>> GetAsync(int page, Expression<Func<TDocument, bool>> expression, Tuple<Expression<Func<TDocument, object>>, SortingType> sort = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;

            IFindFluent<TDocument, TDocument> findFluent;
            if (IsInTransaction())
                findFluent = Collection.Find(this.clientSessionHandle, expression);
            else
                findFluent = Collection.Find(expression);

            var find = findFluent
                .Skip(currentPage * 1000)
                .Limit(1000);

            if (sort != default(Tuple<Expression<Func<TDocument, object>>, SortingType>))
            {
                var builder = new SortDefinitionBuilder<TDocument>();
                var sortByDefinition = sort.Item2 == SortingType.Ascending ? builder.Ascending(sort.Item1) : builder.Descending(sort.Item1);
                find = find.Sort(sortByDefinition);
            }

            return find.ToListAsync(cancellationToken);
        }

        public Task<IAsyncCursor<TDocument>> GetAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsInTransaction())
                return Collection.FindAsync(this.clientSessionHandle, expression, this.configurationSource.Model.FindOptions);
            else
                return Collection.FindAsync(expression, this.configurationSource.Model.FindOptions);
        }

        public Task<TDocument> FindAsync<TKey>(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            IAsyncCursor<TDocument> findSync;
            var bsonDocument = typeof(TDocument).FindKey().GetBsonValue(id);
            if (IsInTransaction())
                findSync = Collection.FindSync(this.clientSessionHandle, bsonDocument);
            else
                findSync = Collection.FindSync(bsonDocument);

            return findSync.FirstOrDefaultAsync(cancellationToken);
        }

        public IMongoQueryable<TDocument> AsQueryable(AggregateOptions aggregateOptions = default)
        {            
            return Collection.AsQueryable(aggregateOptions);
        }

        public Task<List<TDocument>> GetAllAsync(int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;

            IFindFluent<TDocument, TDocument> findFluent;
            if (IsInTransaction())
                findFluent = Collection.Find(this.clientSessionHandle, FilterDefinition<TDocument>.Empty);
            else
                findFluent = Collection.Find(FilterDefinition<TDocument>.Empty);

            var find = findFluent
                .Skip(currentPage * 1000)
                .Limit(1000);

            return find.ToListAsync(cancellationToken);
        }

        public async Task<TDocument> AddAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task task;
            if (IsInTransaction())
                task = Collection.InsertOneAsync(this.clientSessionHandle, item, cancellationToken:cancellationToken);
            else
                task = Collection.InsertOneAsync(item, cancellationToken: cancellationToken);
            
#if NETFULL
            await task.ConfigureAwait(false);
#else
            await task;
#endif
            return item;
        }

        public Task AddRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsInTransaction())
                return Collection.InsertManyAsync(this.clientSessionHandle, documents, cancellationToken: cancellationToken);
            else
                return Collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        }

        public Task ReplaceOneAsync(TDocument item, Expression<Func<TDocument, bool>> filter, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = updateOptionsAction == null ? default(UpdateOptions) : new UpdateOptions();
            updateOptionsAction?.Invoke(options);

            if (IsInTransaction())
                return Collection.ReplaceOneAsync(this.clientSessionHandle, filter, item, options, cancellationToken: cancellationToken);
            else
                return Collection.ReplaceOneAsync(filter, item, options, cancellationToken: cancellationToken);
        }

        public Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> updateDefinitionAction, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = updateOptionsAction == default(Action<UpdateOptions>) ? default(UpdateOptions) : new UpdateOptions();
            var updateBuilder = new UpdateDefinitionBuilder<TDocument>();
            var updateDefinition = updateDefinitionAction(updateBuilder);
            updateOptionsAction?.Invoke(options);

            if (IsInTransaction())
                return Collection.UpdateOneAsync(this.clientSessionHandle, filter, updateDefinition, options, cancellationToken: cancellationToken);
            else
                return Collection.UpdateOneAsync(filter, updateDefinition, options, cancellationToken: cancellationToken);
        }

        public Task UpdateManyAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> updateDefinitionAction, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = updateOptionsAction == default(Action<UpdateOptions>) ? default(UpdateOptions) : new UpdateOptions();
            var updateBuilder = new UpdateDefinitionBuilder<TDocument>();
            var updateDefinition = updateDefinitionAction(updateBuilder);
            updateOptionsAction?.Invoke(options);

            if (IsInTransaction())
                return Collection.UpdateManyAsync(this.clientSessionHandle, filter, updateDefinition, options, cancellationToken);
            else
                return Collection.UpdateManyAsync(filter, updateDefinition, options, cancellationToken);
        }

        public Task DeleteOneAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsInTransaction())
                return Collection.DeleteOneAsync(this.clientSessionHandle, filter, cancellationToken: cancellationToken);
            else
                return Collection.DeleteOneAsync(filter, cancellationToken);
        }
        
        public Task DeleteManyAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsInTransaction())
                return Collection.DeleteManyAsync(this.clientSessionHandle, filter, cancellationToken: cancellationToken);
            else
                return Collection.DeleteManyAsync(filter, cancellationToken);
        }

        public Task DeleteManyByIdAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken))
        {
            var property = typeof(TDocument).FindKey();
            if (IsInTransaction())
                return Collection.DeleteManyAsync(this.clientSessionHandle, Builders<TDocument>.Filter.In(property.Name, documents.Select(c => c.FindKey().GetValueFromProperty(c))), cancellationToken: cancellationToken);
            else
                return Collection.DeleteManyAsync(Builders<TDocument>.Filter.In(property.Name, documents.Select(c => c.FindKey().GetValueFromProperty(c))), cancellationToken);
        }

        public Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsInTransaction())
                return Collection.CountDocumentsAsync(this.clientSessionHandle, FilterDefinition<TDocument>.Empty, cancellationToken: cancellationToken);
            else
                return Collection.CountDocumentsAsync(FilterDefinition<TDocument>.Empty, cancellationToken: cancellationToken);
        }

        public Task<long> CountAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsInTransaction())
                return Collection.CountDocumentsAsync(this.clientSessionHandle, filter, cancellationToken: cancellationToken);
            else
                return Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        }

        public Task<long> EstimatedDocumentCountAsync(Action<EstimatedDocumentCountOptions> estimatedDocumentCountOptionsAction = default(Action<EstimatedDocumentCountOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var estimatedDocumentCountOptions = estimatedDocumentCountOptionsAction == default(Action<EstimatedDocumentCountOptions>) ? default(EstimatedDocumentCountOptions) : new EstimatedDocumentCountOptions();
            estimatedDocumentCountOptionsAction?.Invoke(estimatedDocumentCountOptions);
            return Collection.EstimatedDocumentCountAsync(estimatedDocumentCountOptions, cancellationToken: cancellationToken);
        }

        public async Task<List<TProjection>> MapReduceAsync<TProjection>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TProjection> options, CancellationToken cancellationToken = default(CancellationToken))
        {
            IAsyncCursor<TProjection> mapReduce;
            Task<IAsyncCursor<TProjection>> task;
            if (IsInTransaction())
                task = Collection.MapReduceAsync(this.clientSessionHandle, map, reduce, options, cancellationToken);
            else
                task = Collection.MapReduceAsync(map, reduce, options, cancellationToken);

#if NETFULL
            mapReduce = await task.ConfigureAwait(false);
#else
            mapReduce = await task;
#endif

#if NETFULL
            return await mapReduce.ToListAsync(cancellationToken).ConfigureAwait(false);
#else
            return await mapReduce.ToListAsync(cancellationToken);
#endif
        }

        private async Task RunTransactionAsync(Func<CancellationToken, Task> txnFunc, IClientSessionHandle session, IClientSessionHandle parentSession = default(IClientSessionHandle), Action<Behavior> transactionBehaviorAction = default(Action<Behavior>), CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var transactionBehavior = new Behavior();
                transactionBehaviorAction?.Invoke(transactionBehavior);
                transactionBehavior = transactionBehaviorAction == null ? this.configurationSource.Model.TransactionBehavior : default(Behavior);

                if (parentSession != null)
                {
                    session.AdvanceClusterTime(parentSession.ClusterTime);
                    session.AdvanceOperationTime(parentSession.OperationTime);
                }

                session.StartTransaction(transactionBehavior.ToTransactionOptions());
#if NETFULL
                await txnFunc(cancellationToken).ConfigureAwait(false);

                await commitRetryPolicy.ExecuteAsync((cToken) => CommitAsync(session, cToken), cancellationToken).ConfigureAwait(false);
#else
                await txnFunc(cancellationToken);

                await commitRetryPolicy.ExecuteAsync((cToken) => CommitAsync(session, cToken), cancellationToken);
#endif
            }
            catch (Exception)
            {
#if NETFULL
                await session.AbortTransactionAsync(cancellationToken).ConfigureAwait(false);
#else
                await session.AbortTransactionAsync(cancellationToken);
#endif
                throw;
            }
        }

        private bool IsInTransaction()
        {
            try
            {
                var isInTransaction = this.clientSessionHandle?.IsInTransaction ?? false;
                if (!isInTransaction)
                {
                    Collection = this.configurationSource?.ToMongoCollection(Database);
                    return false;
                }

                Collection = this.clientSessionHandle.ToCollection(configurationSource);

                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        private async Task CommitAsync(IClientSessionHandle session, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
#if NETFULL
                await session.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
#else
                await session.CommitTransactionAsync(cancellationToken);
#endif
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose()
        {
            this.configurationSource?.Dispose();
            this.clientSessionHandle?.Dispose();
            this.Client = null;
            this.Database = null;
            this.Collection = null;
        }
    }
}
