using MongoDB.Bson;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Enums;
using MongoDbFramework.IntegrationTests.Fixtures;
using MongoDbFramework.IntegrationTests.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    public class SharedFileOperationTests<TContext> where TContext : MongoDbContext
    {
        private readonly IoCResolver ioCResolver;
        private TContext context;
        private MongoFileCollection<ImageBlob> imageBlobCollection;

        public SharedFileOperationTests(SocialContextFixture<TContext> fixture, AutofacSocialContextFixture<TContext> autofacFixture, CastleWindsorSocialContextFixture<TContext> castleWindsorFixture)
        {
            this.ioCResolver = IoCResolver.Instance(Tuple.Create(fixture.Container, castleWindsorFixture.Container, autofacFixture.Container));
        }

        public async Task UploadAndDeleteFile(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            byte[] fileBytes = default(byte[]);

#if NET461
            fileBytes = File.ReadAllBytes(filePath);
#else
            fileBytes = await File.ReadAllBytesAsync(filePath);
#endif

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await imageBlobCollection.UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            await this.imageBlobCollection.DeleteAsync(fileSaved);

            var fileById = await this.imageBlobCollection.GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }
        
        public async Task UploadAndGetFileById(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            byte[] fileBytes = default(byte[]);

#if NET461
            fileBytes = File.ReadAllBytes(filePath);
#else
            fileBytes = await File.ReadAllBytesAsync(filePath);
#endif

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await this.imageBlobCollection.UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            var fileById = await this.imageBlobCollection.GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileById);

            await this.imageBlobCollection.DeleteAsync(fileSaved);

            fileById = await this.imageBlobCollection.GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }
        
        public async Task GetAllFiles(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            byte[] fileBytes = default(byte[]);

#if NET461
            fileBytes = File.ReadAllBytes(filePath);
#else
            fileBytes = await File.ReadAllBytesAsync(filePath);
#endif

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await this.imageBlobCollection.UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            var files = await this.imageBlobCollection.GetFilesAllAsync();

            Assert.NotNull(files);
            Assert.True(files.Any());
        }
        
        public async Task RenameFile(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            byte[] fileBytes = default(byte[]);

#if NET461
            fileBytes = File.ReadAllBytes(filePath);
#else
            fileBytes = await File.ReadAllBytesAsync(filePath);
#endif

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };
            var expectedFileName = "image2.jpg";

            var fileSaved = await this.imageBlobCollection.UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            await this.imageBlobCollection.RenameAsync(fileSaved, expectedFileName);

            var fileInfo = await this.imageBlobCollection.GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == expectedFileName);
        }
        
        public async Task GetByFileName(IoCType ioCType)
        {
            this.SetTestContext(ioCType);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            byte[] fileBytes = default(byte[]);

#if NET461
            fileBytes = File.ReadAllBytes(filePath);
#else
            fileBytes = await File.ReadAllBytesAsync(filePath);
#endif

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await this.imageBlobCollection.UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            var fileInfo = await this.imageBlobCollection.GetFileByNameAsync("image.jpg");

            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == "image.jpg");
        }
        
        private void SetTestContext(IoCType ioCType)
        {
            this.context = this.ioCResolver.Resolve<TContext>(ioCType);            
            this.imageBlobCollection = this.context.FileCollection<ImageBlob>();
        }
    }
}
