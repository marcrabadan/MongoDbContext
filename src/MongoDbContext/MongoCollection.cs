using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{
    public class MongoCollection<TDocument> : IMongoCollection<TDocument> where TDocument : Document
    {
        private readonly ConfigurationSource<TDocument> configurationSource;

        public MongoCollection(ConfigurationSource<TDocument> configurationSource)
        {
            this.configurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));

            Client = this.configurationSource?.ToMongoClient();
            Database = this.configurationSource?.ToMongoDatabase(Client);
            Collection = this.configurationSource?.ToMongoCollection(Client);
        }

        protected internal MongoClient Client { get; set; }
        protected internal IMongoDatabase Database { get; set; }
        protected internal MongoDB.Driver.IMongoCollection<TDocument> Collection { get; set; }
        protected internal GridFSBucket Bucket { get; set; }

        public async Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Collection.Find(expression).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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

            return await find.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TDocument> FindAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Collection.Find(c => c.Id == id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TDocument>> GetAllAsync(int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentPage = page < 0 ? 0 : page - 1;
            var find = Collection.Find(FilterDefinition<TDocument>.Empty)
                .Skip(currentPage * 1000)
                .Limit(1000);

            return await find.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TDocument> AddAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            item.Created();
            await Collection.InsertOneAsync(item, cancellationToken: cancellationToken).ConfigureAwait(false);

            return item;
        }

        public async Task AddRangeAsync(List<TDocument> documents, CancellationToken cancellationToken = default(CancellationToken))
        {
            documents.ForEach(c => c.Created());
            await Collection.InsertManyAsync(documents, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            item.Modified();
            await Collection.ReplaceOneAsync(c => c.Id == item.Id, item, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TDocument item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Collection.DeleteOneAsync(c => c.Id == item.Id, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<TProjection>> MapReduceAsync<TProjection>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TProjection> options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapReduce = await Collection.MapReduceAsync(map, reduce, options, cancellationToken).ConfigureAwait(false);
            return await mapReduce.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            this.configurationSource?.Dispose();
            this.Client = null;
            this.Database = null;
            this.Collection = null;
        }
    }
}
