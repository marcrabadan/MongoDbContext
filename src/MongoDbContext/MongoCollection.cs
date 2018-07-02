using Inflector;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbContext.Documents;
using MongoDbContext.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDbContext.Enums;

namespace MongoDbContext
{
    public class MongoCollection<TDocument> : IMongoCollection<TDocument> where TDocument : Document
    {
        private readonly MongoClient _client;
        private MongoDB.Driver.IMongoCollection<TDocument> _collection;

        public MongoCollection(ConfigurationSource<TDocument> configurationSource)
        {
            _client = configurationSource.Source;
            _collection = SetCollection(configurationSource);
        }

        public async Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _collection.Find(expression).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TDocument>> GetAsync(int page, Expression<Func<TDocument, bool>> expression, Tuple<Expression<Func<TDocument, object>>, SortingType> sort = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;
            var find = _collection.Find(expression)
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
            return await _collection.Find(c => c.Id == objId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TDocument>> GetAllAsync(int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;
            var find = _collection.Find(FilterDefinition<TDocument>.Empty)
                .Skip(currentPage * 1000)
                .Limit(1000);
            return await find.ToListAsync(cancellationToken);
        }

        public async Task<TDocument> AddAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _collection.InsertOneAsync(item, cancellationToken: cancellationToken);

            return item;
        }

        public async Task AddRangeAsync(List<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _collection.ReplaceOneAsync(c => c.Id == item.Id, item, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _collection.DeleteOneAsync(c => c.Id == item.Id, cancellationToken: cancellationToken);
        }

        public async Task<List<TProjection>> MapReduceAsync<TProjection>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TProjection> options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapReduce = await _collection.MapReduceAsync(map, reduce, options, cancellationToken);
            return await mapReduce.ToListAsync(cancellationToken);
        }

        private MongoDB.Driver.IMongoCollection<TDocument> SetCollection(ConfigurationSource<TDocument> configurationSource)
        {
            if (configurationSource.Model != default(Model<TDocument>))
            {
                return _client.GetDatabase(configurationSource.Model.DatabaseName)
                    .GetCollection<TDocument>(configurationSource.Model.CollectionName);
            }
            else
            {
                var dbName = string.Format("{0}db", typeof(TDocument).Name.ToLower());
                return _client.GetDatabase(dbName)
                    .GetCollection<TDocument>(typeof(TDocument).Name.ToLower().Pluralize());
            }
        }
    }
}
