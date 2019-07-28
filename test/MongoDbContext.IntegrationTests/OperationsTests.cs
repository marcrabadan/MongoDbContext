using MongoDbFramework.IntegrationTests.Contexts;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Enums;
using MongoDbFramework.IntegrationTests.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    [Trait("Category", "Operations")]
    public class OperationsTests : SharedOperationTests<SocialContext>,
        IOperationTests,
        IClassFixture<SocialContextFixture<SocialContext>>,
        IClassFixture<AutofacSocialContextFixture<SocialContext>>,
        IClassFixture<CastleWindsorSocialContextFixture<SocialContext>>,
        IDisposable
    {
        private readonly SocialContextFixture<SocialContext> fixture;
        private readonly AutofacSocialContextFixture<SocialContext> autofacFixture;
        private readonly CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture;

        public OperationsTests(
            SocialContextFixture<SocialContext> fixture,
            AutofacSocialContextFixture<SocialContext> autofacFixture,
            CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture)
            : base(fixture, autofacFixture, castleWindsorFixture)
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
            await this.IndexAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAddItemToDatabase(IoCType ioCType)
        {
            await this.AddAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAddRangeItemsToDatabase(IoCType ioCType)
        {
            await this.AddRangeAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAsQueryableFilterToDatabase(IoCType ioCType)
        {
            await this.AsQueryableFilter(ioCType);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAsQueryableSortToDatabase(IoCType ioCType)
        {
            await this.AsQueryableSort(ioCType);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldAsQueryablePagingToDatabase(IoCType ioCType)
        {
            await this.AsQueryablePaging(ioCType);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldDeleteItemFromDatabase(IoCType ioCType)
        {
            await this.DeleteAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetAllItemsFromDatabase(IoCType ioCType)
        {
            await this.GetAllAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetFirstOrDefaultItemFromDatabase(IoCType ioCType)
        {
            await this.FirstOrDefaultAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetItemsFromDatabase(IoCType ioCType)
        {
            await this.GetAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldMapReduceOperation(IoCType ioCType)
        {
            await this.MapReduceAsync(ioCType).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldUpdateItemFromDatabase(IoCType ioCType)
        {
            await this.UpdateAsync(ioCType).ConfigureAwait(false);
        }

        public async void Dispose()
        {
            await this.CleanAsync(IoCType.MicrosoftExtensionsDependencyInjection).ConfigureAwait(false);
        }
    }
}
