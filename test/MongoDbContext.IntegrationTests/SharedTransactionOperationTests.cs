using FluentAssertions;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Enums;
using MongoDbFramework.IntegrationTests.Fixtures;
using MongoDbFramework.IntegrationTests.Utils;
using System;
using System.Threading.Tasks;

namespace MongoDbFramework.IntegrationTests
{
    public class SharedTransactionOperationTests<TContext> where TContext : MongoDbContext
    {
        private readonly IoCResolver ioCResolver;
        private TContext context;
        private MongoCollection<Tweet> tweetCollection;

        public SharedTransactionOperationTests(SocialContextFixture<TContext> fixture, AutofacSocialContextFixture<TContext> autofacFixture, CastleWindsorSocialContextFixture<TContext> castleWindsorFixture)
        {
            this.ioCResolver = IoCResolver.Instance(Tuple.Create(fixture.Container, castleWindsorFixture.Container, autofacFixture.Container));
        }

        public async Task CleanAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);

            await this.tweetCollection.DeleteManyAsync(c => true).ConfigureAwait(false);
        }

        public async Task ShouldCommitOperationsAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var id = Guid.NewGuid();

            var tweet = new Tweet
            {
                Id = id,
                Message = "Message"
            };

            using(var session = await this.tweetCollection.BeginSessionAsync())
            {
                try
                {
                    session.StartTransaction();

                    await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

                    var findTweet = await this.tweetCollection.FindAsync(id).ConfigureAwait(false);
                    findTweet.Should().BeNull();

                    var updatedMessage = "UpdatedMessage";
                    tweet.Message = updatedMessage;

                    await this.tweetCollection.UpdateOneAsync(c => c.Id == tweet.Id, c => c.Set(p => p.Message, updatedMessage)).ConfigureAwait(false);

                    findTweet = await this.tweetCollection.FindAsync(id).ConfigureAwait(false);
                    findTweet.Should().BeNull();

                    await session.CommitTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync().ConfigureAwait(false);
                }  
            }

            tweet = await this.tweetCollection.FindAsync(id).ConfigureAwait(false);
            tweet.Should().NotBeNull();
        }

        public async Task ShouldRollbackOperationsAsync(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var id = Guid.NewGuid();

            var tweet = new Tweet
            {
                Id = id,
                Message = "Message"
            };

            using (var session = await this.tweetCollection.BeginSessionAsync())
            {
                try
                {
                    await this.tweetCollection.AddAsync(tweet).ConfigureAwait(false);

                    var findTweet = await this.tweetCollection.FindAsync(id).ConfigureAwait(false);
                    findTweet.Should().BeNull();

                    var updatedMessage = "UpdatedMessage";
                    tweet.Message = updatedMessage;

                    await this.tweetCollection.UpdateOneAsync(c => c.Id == tweet.Id, c => c.Set(p => p.Message, updatedMessage)).ConfigureAwait(false);

                    findTweet = await this.tweetCollection.FindAsync(id).ConfigureAwait(false);
                    findTweet.Should().BeNull();

                    await session.AbortTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync().ConfigureAwait(false);
                }
            }

            tweet = await this.tweetCollection.FindAsync(id).ConfigureAwait(false);
            tweet.Should().BeNull();
        }
        
        private void SetTestContext(IoCType ioCType)
        {
            this.context = this.ioCResolver.Resolve<TContext>(ioCType);
            this.tweetCollection = this.context.Collection<Tweet>();
        }
    }
}
