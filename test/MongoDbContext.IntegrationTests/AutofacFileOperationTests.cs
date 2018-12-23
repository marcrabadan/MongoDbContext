using MongoDB.Bson;
using MongoDbFramework.IntegrationTests.Contexts;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Fixtures;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    [Trait("IoC", "Autofac")]
    [Trait("Category", "FileOperations")]
    public class AutofacFileOperationTests : IClassFixture<AutofacSocialContextFixture<SocialContext>>
    {
        private readonly SocialContext _context;

        public AutofacFileOperationTests(AutofacSocialContextFixture<SocialContext> fixture)
        {
            _context = fixture.Context;
        }

        [Fact]
        public async Task AutofacUploadAndDeleteFile()
        {
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

            var fileSaved = await _context.Images.UploadAsync(file);
            
            Assert.True(fileSaved != ObjectId.Empty);

            await _context.Images.DeleteAsync(fileSaved);

            var fileById = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }
        
        [Fact]
        public async Task AutofacUploadAndGetFileById()
        {
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

            var fileSaved = await _context.Images.UploadAsync(file);
            
            Assert.True(fileSaved != ObjectId.Empty);

            var fileById = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileById);

            await _context.Images.DeleteAsync(fileSaved);

            fileById = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }

        [Fact]
        public async Task AutofacGetAllFiles()
        {
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

            var fileSaved = await _context.Images.UploadAsync(file);
            
            Assert.True(fileSaved != ObjectId.Empty);
            
            var files = await _context.Images.GetFilesAllAsync();

            Assert.NotNull(files);
            Assert.True(files.Any());
        }

        [Fact]
        public async Task AutofacRenameFile()
        {
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

            var fileSaved = await _context.Images.UploadAsync(file);
            
            Assert.True(fileSaved != ObjectId.Empty);

            await _context.Images.RenameAsync(fileSaved, expectedFileName);
            
            var fileInfo = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == expectedFileName);
        }

        [Fact]
        public async Task AutofacGetByFileName()
        {
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

            var fileSaved = await _context.Images.UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            var fileInfo = await _context.Images.GetFileByNameAsync("image.jpg");
            
            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == "image.jpg");
        }
    }
}
