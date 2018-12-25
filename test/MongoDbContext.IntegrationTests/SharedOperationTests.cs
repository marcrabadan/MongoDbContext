using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Fixtures;
using MongoDbFramework.IntegrationTests.Mocks;
using MongoDbFramework.IntegrationTests.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    public abstract class SharedOperationTests<TContext> where TContext : MongoDbContext
    {
        public TContext Context { get; set; }

        public async Task AddAsync()
        {
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await this.Context.Collection<Tweet>().AddAsync(tweet).ConfigureAwait(false);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            await this.Context.Collection<Tweet>().DeleteAsync(added).ConfigureAwait(false);

            var find = await this.Context.Collection<Tweet>().FindAsync(added.Id).ConfigureAwait(false);

            Assert.Null(find);
        }
        
        public async Task UpdateAsync()
        {
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await this.Context.Collection<Tweet>().AddAsync(tweet).ConfigureAwait(false);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            added.Message = "Hi All!!!";

            await this.Context.Collection<Tweet>().UpdateAsync(added).ConfigureAwait(false);

            var findUpdated = await this.Context.Collection<Tweet>().FindAsync(added.Id).ConfigureAwait(false);

            Assert.True(findUpdated.Id == added.Id);
            Assert.True(findUpdated.Message == "Hi All!!!");

            await this.Context.Collection<Tweet>().DeleteAsync(added).ConfigureAwait(false);

            var find = await this.Context.Collection<Tweet>().FindAsync(added.Id).ConfigureAwait(false);

            Assert.Null(find);
        }
        
        public async Task DeleteAsync()
        {
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await this.Context.Collection<Tweet>().AddAsync(tweet).ConfigureAwait(false);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            await this.Context.Collection<Tweet>().DeleteAsync(added).ConfigureAwait(false);

            var find = await this.Context.Collection<Tweet>().FindAsync(added.Id).ConfigureAwait(false);

            Assert.Null(find);
        }
        
        public async Task GetAllAsync()
        {
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
                var added = await this.Context.Collection<Tweet>().AddAsync(tweet).ConfigureAwait(false);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            var query = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);
            var data = query.ToList();

            Assert.NotEmpty(data);

            foreach (var item in data)
            {
                await this.Context.Collection<Tweet>().DeleteAsync(item).ConfigureAwait(false);
            }

            query = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);
            data = query.ToList();

            Assert.Empty(data);
        }
        
        public async Task GetAsync()
        {
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
                var added = await this.Context.Collection<Tweet>().AddAsync(tweet).ConfigureAwait(false);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            var data = await this.Context.Collection<Tweet>().GetAsync(1, c => c.Message == "Message1").ConfigureAwait(false);

            Assert.NotEmpty(data);
            Assert.True(data.Count == 1);

            var query = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);
            data = query.ToList();

            foreach (var item in data)
            {
                await this.Context.Collection<Tweet>().DeleteAsync(item).ConfigureAwait(false);
            }

            query = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);
            data = query.ToList();

            Assert.Empty(data);
        }
        
        public async Task FirstOrDefaultAsync()
        {
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
                var added = await this.Context.Collection<Tweet>().AddAsync(tweet).ConfigureAwait(false);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            Assert.NotNull(await this.Context.Collection<Tweet>().FirstOrDefaultAsync(c => c.Message == "Message1").ConfigureAwait(false));

            var query = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);
            var data = query.ToList();

            foreach (var item in data)
            {
                await this.Context.Collection<Tweet>().DeleteAsync(item).ConfigureAwait(false);
            }

            query = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);
            data = query.ToList();

            Assert.Empty(data);
        }
        
        public async Task AddRangeAsync()
        {
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

            await this.Context.Collection<Tweet>().AddRangeAsync(tweets).ConfigureAwait(false);

            var data = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);

            Assert.NotEmpty(data);
            Assert.True(data.Count == tweets.Count);

            foreach (var item in data)
            {
                await this.Context.Collection<Tweet>().DeleteAsync(item).ConfigureAwait(false);
            }

            data = await this.Context.Collection<Tweet>().GetAllAsync(1).ConfigureAwait(false);

            Assert.Empty(data);
        }
        
        public async Task IndexAsync()
        {
            var movies = MovieMock.GetMovieMocks();
            var expectedIndices = new List<string>
            {
                "_id",
                "CategoryIndex"
            };

            await this.Context.Collection<Movie>().AddRangeAsync(movies).ConfigureAwait(false);

            var data = await this.Context.Collection<Movie>().GetAllAsync(1).ConfigureAwait(false);
            
            var indexManager = this.Context.Collection<Movie>().Collection.Indexes;

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

            foreach (var item in data)
            {
                await this.Context.Collection<Movie>().DeleteAsync(item).ConfigureAwait(false);
            }
        }
        
        public async Task MapReduceAsync()
        {
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

            var moviesList = await this.Context.Collection<Movie>().GetAllAsync(1).ConfigureAwait(false);
            foreach (var movie in moviesList)
            {
                await this.Context.Collection<Movie>().DeleteAsync(movie).ConfigureAwait(false);
            }

            await this.Context.Collection<Movie>().AddRangeAsync(movies).ConfigureAwait(false);

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

            var statistics = await this.Context.Collection<Movie>().MapReduceAsync(map, reduce, options).ConfigureAwait(false);

            foreach (var item in statistics)
            {
                var expectedMovie = expected.FirstOrDefault(c => c.Id == item.Id);
                if (expectedMovie != null)
                {
                    Assert.True(expectedMovie.value.Count == item.value.Count);
                    Assert.True(expectedMovie.value.TotalMinutes == item.value.TotalMinutes);
                    Assert.True(expectedMovie.value.Average == item.value.Average);
                }
            }
        }
    }
}
