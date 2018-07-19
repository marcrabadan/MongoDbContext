using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{
    public interface IMongoCollection<TDocument> where TDocument : Document
    {
        Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TDocument>> GetAsync(int page, Expression<Func<TDocument, bool>> expression, Tuple<Expression<Func<TDocument, object>>, SortingType> sort = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<TDocument> FindAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TDocument>> GetAllAsync(int page, CancellationToken cancellationToken = default(CancellationToken));
        Task<TDocument> AddAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken));
        Task AddRangeAsync(List<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TProjection>> MapReduceAsync<TProjection>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TProjection> options, CancellationToken cancellationToken = default(CancellationToken));
    }
}
