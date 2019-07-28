using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDbFramework.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{
    public interface IMongoCollection<TDocument> : IDisposable where TDocument : IDocument
    {
        Task<IClientSessionHandle> BeginSessionAsync(Action<SessionBehavior> sessionBehaviorAction = default(Action<SessionBehavior>), CancellationToken cancellationToken = default(CancellationToken));
        Task DoTransactionAsync(Func<CancellationToken, Task> txnAction, Action<Behavior> transactionBehaviorAction = default(Action<Behavior>), Action<SessionBehavior> sessionBehaviorAction = default(Action<SessionBehavior>), IClientSessionHandle parentSession = default(IClientSessionHandle), CancellationToken cancellationToken = default(CancellationToken));
        Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TDocument>> GetAsync(int page, Expression<Func<TDocument, bool>> expression, Tuple<Expression<Func<TDocument, object>>, SortingType> sort = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TDocument> FindAsync<TValue>(TValue id, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TDocument>> GetAllAsync(int page, CancellationToken cancellationToken = default(CancellationToken));
        Task<TDocument> AddAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken));
        Task AddRangeAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken));
        Task ReplaceOneAsync(TDocument item, Expression<Func<TDocument, bool>> filter, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateOneAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> updateDefinitionAction, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateManyAsync(Expression<Func<TDocument, bool>> filter, Func<UpdateDefinitionBuilder<TDocument>, UpdateDefinition<TDocument>> updateDefinitionAction, Action<UpdateOptions> updateOptionsAction = default(Action<UpdateOptions>), CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteOneAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteManyAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));
        Task CountAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CountAsync(Expression<Func<TDocument, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));
        Task EstimatedDocumentCountAsync(Action<EstimatedDocumentCountOptions> estimatedDocumentCountOptionsAction = default(Action<EstimatedDocumentCountOptions>), CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TProjection>> MapReduceAsync<TProjection>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TProjection> options, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteManyByIdAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken));
        IMongoQueryable<TDocument> AsQueryable(AggregateOptions aggregateOptions = default);
    }
}
