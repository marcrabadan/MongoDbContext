using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Enums;
using MongoDbFramework.IntegrationTests.Fixtures;
using MongoDbFramework.IntegrationTests.Mocks;
using MongoDbFramework.IntegrationTests.Projections;
using MongoDbFramework.IntegrationTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    public abstract class SharedOperationTests<TContext> where TContext : MongoDbContext
    {
        private readonly IoCResolver ioCResolver;
        private TContext context;
        private MongoCollection<Tweet> tweetCollection;
        private MongoCollection<Movie> movieCollection;

        protected SharedOperationTests(SocialContextFixture<TContext> fixture, AutofacSocialContextFixture<TContext> autofacFixture, CastleWindsorSocialContextFixture<TContext> castleWindsorFixture)
        {
            this.ioCResolver = IoCResolver.Instance(Tuple.Create(fixture.Container, castleWindsorFixture.Container, autofacFixture.Container));            
        }

        public async Task CleanAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            
            await this.tweetCollection.DeleteManyAsync(c => true).ConfigureAwait(false);
            await this.movieCollection.DeleteManyAsync(c => true).ConfigureAwait(false);
        }

        public async Task AddAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);

            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);
        }
        
        public async Task UpdateAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            added.Message = "Hi All!!!";

            await this.tweetCollection.UpdateOneAsync(c => c.Id == added.Id, c => c.Set(x => x.Message, "Hi All!!!")).ConfigureAwait(false);

            var findUpdated = await this.tweetCollection.FindAsync(added.Id).ConfigureAwait(false);

            Assert.True(findUpdated.Id == added.Id);
            Assert.True(findUpdated.Message == "Hi All!!!");
        }
        
        public async Task DeleteAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            await this.tweetCollection.DeleteOneAsync(c => c.Id == added.Id).ConfigureAwait(false);

            var find = await this.tweetCollection.FindAsync(added.Id).ConfigureAwait(false);

            Assert.Null(find);
        }
        
        public async Task GetAllAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var tweets = new List<Tweet>
            {
                new Tweet
                {
                    Message = "Hi!!!"
                },
                new Tweet
                {
                    Message = "Hi!!!"
                }
            };

            foreach (var tweet in tweets)
            {
                var added = await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            var query = await this.tweetCollection.GetAllAsync(1).ConfigureAwait(false);
            var data = query.ToList();

            Assert.NotEmpty(data);
        }
        
        public async Task GetAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var tweets = new List<Tweet>
            {
                new Tweet
                {
                    Message = "Message1"
                },
                new Tweet
                {
                    Message = "Message2"
                }
            };

            foreach (var tweet in tweets)
            {
                var added = await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            var data = await this.tweetCollection.GetAsync(1, c => c.Message == "Message1").ConfigureAwait(false);

            Assert.NotEmpty(data);
            Assert.True(data.Count == 1);
        }
        
        public async Task FirstOrDefaultAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var tweets = new List<Tweet>
            {
                new Tweet
                {
                    Message = "Message1"
                },
                new Tweet
                {
                    Message = "Message2"
                }
            };

            foreach (var tweet in tweets)
            {
                var added = await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            Assert.NotNull(await this.tweetCollection.FirstOrDefaultAsync(c => c.Message == "Message1").ConfigureAwait(false));
        }
        
        public async Task AddRangeAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var tweets = new List<Tweet>
            {
                new Tweet
                {
                    Message = "Message1"
                },
                new Tweet
                {
                    Message = "Message2"
                }
            };

            await this.tweetCollection.AddRangeAsync(tweets).ConfigureAwait(false);

            var data = await this.tweetCollection.GetAllAsync(1).ConfigureAwait(false);

            Assert.NotEmpty(data);
            Assert.True(data.Count == tweets.Count);
        }
        
        public async Task IndexAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var movies = MovieMock.GetMovieMocks();
            var expectedIndices = new List<string>
            {
                "_id",
                "CategoryIndex"
            };

            await this.movieCollection.AddRangeAsync(movies).ConfigureAwait(false);

            var data = await this.movieCollection.GetAllAsync(1).ConfigureAwait(false);
            
            var indexManager = this.movieCollection.Collection.Indexes;

            var indices = indexManager.List();
            while (indices.MoveNext())
            {
                var currentIndex = indices.Current;
                foreach (var index in currentIndex)
                {
                    Assert.Contains(index.Elements, c =>
                    {
                        if (index.TryGetValue("name", out var name))
                        {
                            return expectedIndices.Any(x => name.ToString().Contains(x));
                        }

                        return false;
                    });
                }
            }
        }
        
        public async Task MapReduceAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var movies = MovieMock.GetMovieMocks();

            var expected = new List<ReduceResult<MovieProjection>>
            {
                new ReduceResult<MovieProjection>()
                {
                    Id = "Horror",
                    value = new MovieProjection
                    {
                        Count = 2,
                        TotalMinutes = 463,
                        Average = 231.5
                    }
                },
                new ReduceResult<MovieProjection>()
                {
                    Id = "SciFi",
                    value = new MovieProjection
                    {
                        Count = 1,
                        TotalMinutes = 118,
                        Average = 118
                    }
                }
            };

            var data = await this.movieCollection.GetAllAsync(1).ConfigureAwait(false);
            await this.movieCollection.DeleteManyByIdAsync(data).ConfigureAwait(false);
            await this.movieCollection.AddRangeAsync(movies).ConfigureAwait(false);

            BsonJavaScript map = @"
                function() {
                    emit(this.Category, { Count: 1, TotalMinutes: this.Minutes });
                }";

            BsonJavaScript reduce = @"
                function(key, values) {
                    var result = {Count: 0, TotalMinutes: 0 };

                    values.forEach(function(value){               
                        result.Count += value.Count;
                        result.TotalMinutes += value.TotalMinutes;
                    });

                    return result;
                }";

            BsonJavaScript finalice = @"
                function(key, value){
                  value.Average = value.TotalMinutes / value.Count;
                  return value;
                }";

            var options = new MapReduceOptions<Movie, ReduceResult<MovieProjection>>
            {
                Finalize = finalice,
                OutputOptions = MapReduceOutputOptions.Inline
            };

            var statistics = await this.movieCollection.MapReduceAsync(map, reduce, options).ConfigureAwait(false);

            foreach (var item in statistics)
            {
                var expectedMovie = expected.FirstOrDefault(c => c.Id == item.Id);
                if (expectedMovie != null)
                {
                    Assert.True(expectedMovie.value.Count <= item.value.Count);
                    Assert.True(expectedMovie.value.TotalMinutes <= item.value.TotalMinutes);
                    Assert.True(expectedMovie.value.Average <= item.value.Average);
                }
            }
        }

        private void SetTestContext(IoCType ioCType)
        {
            this.context = this.ioCResolver.Resolve<TContext>(ioCType);
            this.tweetCollection = this.context.Collection<Tweet>();
            this.movieCollection = this.context.Collection<Movie>();
        }
    }
}
