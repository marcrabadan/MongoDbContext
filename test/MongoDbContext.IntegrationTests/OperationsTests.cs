using System;
using System.Threading.Tasks;
using MongoDbFramework.IntegrationTests.Contexts;
using MongoDbFramework.IntegrationTests.Enums;
using MongoDbFramework.IntegrationTests.Fixtures;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    [Trait("Category", "Operations")]
    public class OperationsTests : SharedOperationTests<SocialContext>,
        IOperationTests,
        IClassFixture<SocialContextFixture<SocialContext>>,
        IClassFixture<AutofacSocialContextFixture<SocialContext>>,
        IClassFixture<CastleWindsorSocialContextFixture<SocialContext>>
    {
        private readonly SocialContextFixture<SocialContext> fixture;
        private readonly AutofacSocialContextFixture<SocialContext> autofacFixture;
        private readonly CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture;
        
        public OperationsTests(
            SocialContextFixture<SocialContext> fixture,
            AutofacSocialContextFixture<SocialContext> autofacFixture,
            CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture)
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            this.autofacFixture = autofacFixture ?? throw new ArgumentNullException(nameof(fixture));
            this.castleWindsorFixture = castleWindsorFixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Theory(DisplayName = "ShouldAddIndexAndRetrieveIt")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAddIndexAndRetrieveIt(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.IndexAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAddItemToDatabase(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.AddAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAddRangeItemsToDatabase(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.AddRangeAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldDeleteItemFromDatabase(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.DeleteAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetAllItemsFromDatabase(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.GetAllAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetFirstOrDefaultItemFromDatabase(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetItemsFromDatabase(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.GetAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldMapReduceOperation(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.MapReduceAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldUpdateItemFromDatabase(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.UpdateAsync().ConfigureAwait(false);
        }

        private void SetContext(IoCType ioCType)
        {
            switch (ioCType)
            {
                case IoCType.MicrosoftExtensionsDependencyInjection:
                    this.Context = this.fixture.Context;
                    break;
                case IoCType.Autofac:
                    this.Context = this.autofacFixture.Context;
                    break;
                case IoCType.CastleWindsor:
                    this.Context = this.castleWindsorFixture.Context;
                    break;
            }
        }
    }
}
