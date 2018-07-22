using Inflector;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.GridFS;

namespace MongoDbFramework
{
    public class MongoCollection<TDocument> : IMongoCollection<TDocument> where TDocument : Document
    {
        public MongoCollection(ConfigurationSource<TDocument> configurationSource)
        {
            Client = configurationSource.ToMongoClient();
            Database = configurationSource.ToMongoDatabase(Client);
            Collection = configurationSource.ToMongoCollection(Client);
        }
        
        internal MongoClient Client { get; }
        internal IMongoDatabase Database { get; }
        internal MongoDB.Driver.IMongoCollection<TDocument> Collection { get; }
        internal GridFSBucket Bucket { get; }

        public async Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Collection.Find(expression).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TDocument>> GetAsync(int page, Expression<Func<TDocument, bool>> expression, Tuple<Expression<Func<TDocument, object>>, SortingType> sort = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;
            var find = Collection.Find(expression)
                .Skip(currentPage * 1000)
                .Limit(1000);

            if (sort != default(Tuple<Expression<Func<TDocument, object>>, SortingType>))
            {
                var builder = new SortDefinitionBuilder<TDocument>();
                var sortByDefinition = sort.Item2 == SortingType.Ascending ? builder.Ascending(sort.Item1) : builder.Descending(sort.Item1);
                find = find.Sort(sortByDefinition);
            }

            return await find.ToListAsync(cancellationToken);
        }

        public async Task<TDocument> FindAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var objId = ObjectId.Parse(id);
            return await Collection.Find(c => c.Id == objId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TDocument>> GetAllAsync(int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;
            var find = Collection.Find(FilterDefinition<TDocument>.Empty)
                .Skip(currentPage * 1000)
                .Limit(1000);
            return await find.ToListAsync(cancellationToken);
        }

        public async Task<TDocument> AddAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            item.Created();
            await Collection.InsertOneAsync(item, cancellationToken: cancellationToken);

            return item;
        }

        public async Task AddRangeAsync(List<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken))
        {
            documents.ForEach(c => c.Created());
            await Collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            item.Modified();
            await Collection.ReplaceOneAsync(c => c.Id == item.Id, item, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Collection.DeleteOneAsync(c => c.Id == item.Id, cancellationToken: cancellationToken);
        }

        public async Task<List<TProjection>> MapReduceAsync<TProjection>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TProjection> options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapReduce = await Collection.MapReduceAsync(map, reduce, options, cancellationToken);
            return await mapReduce.ToListAsync(cancellationToken);
        }
    }
}
