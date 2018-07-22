using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDbFramework.IntegrationTests.Contexts;
using MongoDbFramework.IntegrationTests.Documents;
using MongoDbFramework.IntegrationTests.Fixtures;
using MongoDB.Bson;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    public class FileOperationTests : IClassFixture<SocialContextFixture<SocialContext>>
    {
        private readonly SocialContext _context;

        public FileOperationTests(SocialContextFixture<SocialContext> fixture)
        {
            _context = fixture.Context;
        }

        [Fact]
        public async Task UploadAndDeleteFile()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await _context.Images.UploadAsync(file);

            Assert.NotEmpty(fileSaved);
            Assert.True(ObjectId.TryParse(fileSaved, out var objectId));
            Assert.True(objectId != ObjectId.Empty);

            await _context.Images.DeleteAsync(fileSaved);

            var fileById = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }
        
        [Fact]
        public async Task UploadAndGetFileById()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await _context.Images.UploadAsync(file);

            Assert.NotEmpty(fileSaved);
            Assert.True(ObjectId.TryParse(fileSaved, out var objectId));
            Assert.True(objectId != ObjectId.Empty);

            var fileById = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileById);

            await _context.Images.DeleteAsync(fileSaved);

            fileById = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }

        [Fact]
        public async Task GetAllFiles()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await _context.Images.UploadAsync(file);

            Assert.NotEmpty(fileSaved);
            Assert.True(ObjectId.TryParse(fileSaved, out var objectId));
            Assert.True(objectId != ObjectId.Empty);
            
            var files = await _context.Images.GetFilesAllAsync();

            Assert.NotNull(files);
            Assert.True(files.Any());
        }

        [Fact]
        public async Task RenameFile()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };
            var expectedFileName = "image2.jpg";

            var fileSaved = await _context.Images.UploadAsync(file);

            Assert.NotEmpty(fileSaved);
            Assert.True(ObjectId.TryParse(fileSaved, out var objectId));
            Assert.True(objectId != ObjectId.Empty);

            await _context.Images.RenameAsync(fileSaved, expectedFileName);
            
            var fileInfo = await _context.Images.GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == expectedFileName);
        }

        [Fact]
        public async Task GetByFileName()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "image.jpg");

            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var file = new ImageBlob
            {
                FileName = "image.jpg",
                Data = fileBytes
            };

            var fileSaved = await _context.Images.UploadAsync(file);

            Assert.NotEmpty(fileSaved);
            Assert.True(ObjectId.TryParse(fileSaved, out var objectId));
            Assert.True(objectId != ObjectId.Empty);

            var fileInfo = await _context.Images.GetFileByNameAsync("image.jpg");
            
            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == "image.jpg");
        }
    }
}
