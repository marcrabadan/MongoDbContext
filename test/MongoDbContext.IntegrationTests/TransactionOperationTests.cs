using MongoDbFramework.IntegrationTests.Contexts;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Enums;
using MongoDbFramework.IntegrationTests.Fixtures;
using MongoDbFramework.IntegrationTests.Tests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    [Trait("Category", "TransactionOperationTests")]
    public class TransactionOperationTests :
       SharedTransactionOperationTests<SocialContext>,
       ITransactionOperationTests,
       IClassFixture<SocialContextFixture<SocialContext>>,
       IClassFixture<AutofacSocialContextFixture<SocialContext>>,
       IClassFixture<CastleWindsorSocialContextFixture<SocialContext>>,
       IDisposable
    {
        private readonly SocialContextFixture<SocialContext> fixture;
        private readonly AutofacSocialContextFixture<SocialContext> autofacFixture;
        private readonly CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture;

        public TransactionOperationTests(
            SocialContextFixture<SocialContext> fixture,
            AutofacSocialContextFixture<SocialContext> autofacFixture,
            CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture)
            : base(fixture, autofacFixture, castleWindsorFixture)
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            this.autofacFixture = autofacFixture ?? throw new ArgumentNullException(nameof(fixture));
            this.castleWindsorFixture = castleWindsorFixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Theory(DisplayName = "ShouldCommitOperations")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldCommitOperations(IoCType ioCType)
        {
            await this.ShouldCommitOperationsAsync(ioCType).ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldRollbackOperations")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldRollbackOperations(IoCType ioCType)
        {
            await this.ShouldRollbackOperationsAsync(ioCType).ConfigureAwait(false);
        }
        
        public async void Dispose()
        {
            await this.CleanAsync(IoCType.MicrosoftExtensionsDependencyInjection).ConfigureAwait(false);
        }
    }
}
