using MongoDbFramework.IntegrationTests.Contexts;
using MongoDbFramework.IntegrationTests.Enums;
using MongoDbFramework.IntegrationTests.Fixtures;
using MongoDbFramework.IntegrationTests.Tests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    [Trait("Category", "FileOperations")]
    public class FileOperationTests :
        SharedFileOperationTests<SocialContext>,
        IFileOperationTests,
        IClassFixture<SocialContextFixture<SocialContext>>,
        IClassFixture<AutofacSocialContextFixture<SocialContext>>,
        IClassFixture<CastleWindsorSocialContextFixture<SocialContext>>
    {
        private readonly SocialContextFixture<SocialContext> fixture;
        private readonly AutofacSocialContextFixture<SocialContext> autofacFixture;
        private readonly CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture;

        public FileOperationTests(
            SocialContextFixture<SocialContext> fixture,
            AutofacSocialContextFixture<SocialContext> autofacFixture,
            CastleWindsorSocialContextFixture<SocialContext> castleWindsorFixture)
            : base(fixture, autofacFixture, castleWindsorFixture)
        {
            this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            this.autofacFixture = autofacFixture ?? throw new ArgumentNullException(nameof(fixture));
            this.castleWindsorFixture = castleWindsorFixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Theory(DisplayName = "ShouldGetAllFiles")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetAllFiles(IoCType ioCType)
        {
            await this.GetAllFiles(ioCType).ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldGetByFileName")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetByFileName(IoCType ioCType)
        {
            await this.GetByFileName(ioCType).ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldRenameFile")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldRenameFile(IoCType ioCType)
        {
            await this.RenameFile(ioCType).ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldUploadAndDeleteFile")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldUploadAndDeleteFile(IoCType ioCType)
        {
            await this.UploadAndDeleteFile(ioCType).ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldUploadAndGetFileById")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldUploadAndGetFileById(IoCType ioCType)
        {
            await this.UploadAndGetFileById(ioCType).ConfigureAwait(false);
        }
    }
}
