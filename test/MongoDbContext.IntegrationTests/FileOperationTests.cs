﻿using MongoDbFramework.IntegrationTests.Contexts;
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
            this.SetContext(ioCType);
            await this.GetAllFiles().ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldGetByFileName")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldGetByFileName(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.GetByFileName().ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldRenameFile")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldRenameFile(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.RenameFile().ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldUploadAndDeleteFile")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldUploadAndDeleteFile(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.UploadAndDeleteFile().ConfigureAwait(false);
        }

        [Theory(DisplayName = "ShouldUploadAndGetFileById")]
        [InlineData(IoCType.MicrosoftExtensionsDependencyInjection)]
        [InlineData(IoCType.Autofac)]
        [InlineData(IoCType.CastleWindsor)]
        public async Task ShouldUploadAndGetFileById(IoCType ioCType)
        {
            this.SetContext(ioCType);
            await this.UploadAndGetFileById().ConfigureAwait(false);
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
