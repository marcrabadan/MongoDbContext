using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{
    public class MongoCollection<TDocument> : IMongoCollection<TDocument> where TDocument : Document
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
            this.clientSessionHandle = await this.configurationSource.Source.StartSessionAsync(sessionBehavior.ToClientSessionOptions(), cancellationToken).ConfigureAwait(false);
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
                    await commitRetryPolicy.ExecuteAsync((cToken) => RunTransactionAsync(txnAction, session, parentSession, transactionBehaviorAction, cToken), cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    this.clientSessionHandle = default(IClientSessionHandle);
                    throw;
                }
            }
            this.clientSessionHandle = default(IClientSessionHandle);
        }
        
        public async Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            IFindFluent<TDocument, TDocument> findFluent;
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                findFluent = Collection.Find(this.clientSessionHandle, expression);
            else
                findFluent = Collection.Find(expression);

            return await findFluent.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TDocument>> GetAsync(int page, Expression<Func<TDocument, bool>> expression, Tuple<Expression<Func<TDocument, object>>, SortingType> sort = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;

            IFindFluent<TDocument, TDocument> findFluent;
            if (this.clientSessionHandle?.IsInTransaction ?? false)
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

            return await find.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<IAsyncCursor<TDocument>> GetAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                return Collection.FindAsync(this.clientSessionHandle, expression, this.configurationSource.Model.FindOptions);
            else
                return Collection.FindAsync(expression, this.configurationSource.Model.FindOptions);
        }

        public async Task<TDocument> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            IAsyncCursor<TDocument> findSync;
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                findSync = Collection.FindSync(this.clientSessionHandle, c => c.Id == id);
            else
                findSync = Collection.FindSync(c => c.Id == id);

            return await findSync.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TDocument>> GetAllAsync(int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;

            IFindFluent<TDocument, TDocument> findFluent;
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                findFluent = Collection.Find(this.clientSessionHandle, FilterDefinition<TDocument>.Empty);
            else
                findFluent = Collection.Find(FilterDefinition<TDocument>.Empty);

            var find = findFluent
                .Skip(currentPage * 1000)
                .Limit(1000);

            return await find.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TDocument> AddAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            item.Created();

            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.InsertOneAsync(this.clientSessionHandle, item, cancellationToken: CancellationToken.None).ConfigureAwait(false);
            else
                await Collection.InsertOneAsync(item, cancellationToken: cancellationToken).ConfigureAwait(false);

            return item;
        }

        public async Task AddRangeAsync(List<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken))
        {
            documents.ForEach(c => c.Created());

            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.InsertManyAsync(this.clientSessionHandle, documents, cancellationToken: cancellationToken).ConfigureAwait(false);
            else
                await Collection.InsertManyAsync(documents, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task ReplaceOneAsync(TDocument item, Expression<Func<TDocument, bool>> filter, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            item.Modified();
            var options = updateOptionsAction == null ? default(UpdateOptions) : new UpdateOptions();
            updateOptionsAction?.Invoke(options);

            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.ReplaceOneAsync(this.clientSessionHandle, filter, item, options, cancellationToken: cancellationToken).ConfigureAwait(false);
            else
                await Collection.ReplaceOneAsync(filter, item, options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> updateDefinitionAction, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = updateOptionsAction == default(Action<UpdateOptions>) ? default(UpdateOptions) : new UpdateOptions();
            var updateBuilder = new UpdateDefinitionBuilder<TDocument>();
            var updateDefinition = updateDefinitionAction(updateBuilder);
            updateOptionsAction?.Invoke(options);

            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.UpdateOneAsync(this.clientSessionHandle, filter, updateDefinition, options, cancellationToken: cancellationToken).ConfigureAwait(false);
            else
                await Collection.UpdateOneAsync(filter, updateDefinition, options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateManyAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> updateDefinitionAction, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = updateOptionsAction == default(Action<UpdateOptions>) ? default(UpdateOptions) : new UpdateOptions();
            var updateBuilder = new UpdateDefinitionBuilder<TDocument>();
            var updateDefinition = updateDefinitionAction(updateBuilder);
            updateOptionsAction?.Invoke(options);

            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.UpdateManyAsync(this.clientSessionHandle, filter, updateDefinition, options).ConfigureAwait(false);
            else
                await Collection.UpdateManyAsync(filter, updateDefinition, options).ConfigureAwait(false);
        }

        public async Task DeleteOneAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.DeleteOneAsync(this.clientSessionHandle, filter, cancellationToken: cancellationToken).ConfigureAwait(false);
            else
                await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteManyAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.DeleteManyAsync(this.clientSessionHandle, filter, cancellationToken: cancellationToken).ConfigureAwait(false);
            else
                await Collection.DeleteManyAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task CountAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.CountDocumentsAsync(this.clientSessionHandle, FilterDefinition<TDocument>.Empty, cancellationToken: cancellationToken).ConfigureAwait(false);
            else
                await Collection.CountDocumentsAsync(FilterDefinition<TDocument>.Empty, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task CountAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                await Collection.CountDocumentsAsync(this.clientSessionHandle, filter, cancellationToken: cancellationToken).ConfigureAwait(false);
            else
                await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public Task EstimatedDocumentCountAsync(Action<EstimatedDocumentCountOptions> estimatedDocumentCountOptionsAction = default(Action<EstimatedDocumentCountOptions>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var estimatedDocumentCountOptions = estimatedDocumentCountOptionsAction == default(Action<EstimatedDocumentCountOptions>) ? default(EstimatedDocumentCountOptions) : new EstimatedDocumentCountOptions();
            estimatedDocumentCountOptionsAction?.Invoke(estimatedDocumentCountOptions);
            return Collection.EstimatedDocumentCountAsync(estimatedDocumentCountOptions, cancellationToken: cancellationToken);
        }

        public async Task<List<TProjection>> MapReduceAsync<TProjection>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TProjection> options, CancellationToken cancellationToken = default(CancellationToken))
        {
            IAsyncCursor<TProjection> mapReduce;
            if (this.clientSessionHandle?.IsInTransaction ?? false)
                mapReduce = await Collection.MapReduceAsync(this.clientSessionHandle, map, reduce, options, cancellationToken).ConfigureAwait(false);
            else
                mapReduce = await Collection.MapReduceAsync(map, reduce, options, cancellationToken).ConfigureAwait(false);

            return await mapReduce.ToListAsync(cancellationToken).ConfigureAwait(false);
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
                
                await txnFunc(cancellationToken).ConfigureAwait(false);

                await commitRetryPolicy.ExecuteAsync((cToken) => CommitAsync(session, cToken), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                await session.AbortTransactionAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        private async Task CommitAsync(IClientSessionHandle session, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await session.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
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
