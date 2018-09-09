using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbFramework.IntegrationTests.Contexts;
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
    public class OperationsTest : IClassFixture<SocialContextFixture<SocialContext>>
    {
        private readonly SocialContext _context;

        public OperationsTest(SocialContextFixture<SocialContext> fixture)
        {
            _context = fixture.Context;
        }

        [Fact]
        public async Task AddTests()
        {
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await _context.Tweets.AddAsync(tweet);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            await _context.Tweets.DeleteAsync(added);

            var find = await _context.Tweets.FindAsync(added.Id);

            Assert.Null(find);
        }

        [Fact]
        public async Task UpdateTests()
        {
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await _context.Tweets.AddAsync(tweet);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            added.Message = "Hi All!!!";

            await _context.Tweets.UpdateAsync(added);

            var findUpdated = await _context.Tweets.FindAsync(added.Id);

            Assert.True(findUpdated.Id == added.Id);
            Assert.True(findUpdated.Message == "Hi All!!!");

            await _context.Tweets.DeleteAsync(added);

            var find = await _context.Tweets.FindAsync(added.Id);

            Assert.Null(find);
        }

        [Fact]
        public async Task DeleteTests()
        {
            var tweet = new Tweet
            {
                Message = "Hi!!!"
            };

            var added = await _context.Tweets.AddAsync(tweet);

            Assert.NotNull(added);
            Assert.True(added.Id != Guid.Empty);

            await _context.Tweets.DeleteAsync(added);

            var find = await _context.Tweets.FindAsync(added.Id);

            Assert.Null(find);
        }

        [Fact]
        public async Task GetAllTests()
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
                var added = await _context.Tweets.AddAsync(tweet);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            var query = await _context.Tweets.GetAllAsync(1);
            var data = query.ToList();

            Assert.NotEmpty(data);

            foreach (var item in data)
            {
                await _context.Tweets.DeleteAsync(item);
            }

            query = await _context.Tweets.GetAllAsync(1);
            data = query.ToList();

            Assert.Empty(data);
        }

        [Fact]
        public async Task GetTests()
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
                var added = await _context.Tweets.AddAsync(tweet);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            var data = await _context.Tweets.GetAsync(1, c => c.Message == "Message1");

            Assert.NotEmpty(data);
            Assert.True(data.Count == 1);

            var query = await _context.Tweets.GetAllAsync(1);
            data = query.ToList();

            foreach (var item in data)
            {
                await _context.Tweets.DeleteAsync(item);
            }

            query = await _context.Tweets.GetAllAsync(1);
            data = query.ToList();

            Assert.Empty(data);
        }

        [Fact]
        public async Task FirstOrDefaultTests()
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
                var added = await _context.Tweets.AddAsync(tweet);

                Assert.NotNull(added);
                Assert.True(added.Id != Guid.Empty);
            }

            Assert.NotNull(await _context.Tweets.FirstOrDefaultAsync(c => c.Message == "Message1"));

            var query = await _context.Tweets.GetAllAsync(1);
            var data = query.ToList();

            foreach (var item in data)
            {
                await _context.Tweets.DeleteAsync(item);
            }

            query = await _context.Tweets.GetAllAsync(1);
            data = query.ToList();

            Assert.Empty(data);
        }

        [Fact]
        public async Task AddRangeTests()
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

            await _context.Tweets.AddRangeAsync(tweets);

            var data = await _context.Tweets.GetAllAsync(1);

            Assert.NotEmpty(data);
            Assert.True(data.Count == tweets.Count);

            foreach (var item in data)
            {
                await _context.Tweets.DeleteAsync(item);
            }

            data = await _context.Tweets.GetAllAsync(1);

            Assert.Empty(data);
        }

        [Fact]
        public async Task IndexTest()
        {
            var movies = MovieMock.GetMovieMocks();
            var expectedIndices = new List<string>
            {
                "_id",
                "CategoryIndex"
            };

            await _context.Movies.AddRangeAsync(movies);

            var data = await _context.Movies.GetAllAsync(1);

            var collection = _context.Movies as MongoCollection<Movie>;

            var indexManager = collection.Collection.Indexes;

            var indices = indexManager.List();
            while (indices.MoveNext())
            {
                var currentIndex = indices.Current;
                foreach (var index in currentIndex)
                {
                    Assert.Contains(index.Elements, c =>
                    {
                        if(index.TryGetValue("name", out var name))
                        {
                            return expectedIndices.Any(x => name.ToString().Contains(x));
                        }

                        return false;
                    });
                }
            }

            foreach (var item in data)
            {
                await _context.Movies.DeleteAsync(item);
            }
        }

        [Fact]
        public async Task MapReduceTests()
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

            var moviesList = await _context.Movies.GetAllAsync(1);
            foreach (var movie in moviesList)
            {
                await _context.Movies.DeleteAsync(movie);
            }

            await _context.Movies.AddRangeAsync(movies);

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

            var statistics = await _context.Movies.MapReduceAsync(map, reduce, options);

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
