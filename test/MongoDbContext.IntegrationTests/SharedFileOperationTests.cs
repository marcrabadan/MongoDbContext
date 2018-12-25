using MongoDB.Bson;
using MongoDbFramework.IntegrationTests.Documents;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbFramework.IntegrationTests
{
    public class SharedFileOperationTests<TContext> where TContext : MongoDbContext
    {
        public TContext Context { get; set; }

        public async Task UploadAndDeleteFile()
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

            var fileSaved = await this.Context.FileCollection<ImageBlob>().UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            await this.Context.FileCollection<ImageBlob>().DeleteAsync(fileSaved);

            var fileById = await this.Context.FileCollection<ImageBlob>().GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }
        
        public async Task UploadAndGetFileById()
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

            var fileSaved = await this.Context.FileCollection<ImageBlob>().UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            var fileById = await this.Context.FileCollection<ImageBlob>().GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileById);

            await this.Context.FileCollection<ImageBlob>().DeleteAsync(fileSaved);

            fileById = await this.Context.FileCollection<ImageBlob>().GetFileByIdAsync(fileSaved);

            Assert.Null(fileById);
        }
        
        public async Task GetAllFiles()
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

            var fileSaved = await this.Context.FileCollection<ImageBlob>().UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            var files = await this.Context.FileCollection<ImageBlob>().GetFilesAllAsync();

            Assert.NotNull(files);
            Assert.True(files.Any());
        }
        
        public async Task RenameFile()
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

            var fileSaved = await this.Context.FileCollection<ImageBlob>().UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            await this.Context.FileCollection<ImageBlob>().RenameAsync(fileSaved, expectedFileName);

            var fileInfo = await this.Context.FileCollection<ImageBlob>().GetFileByIdAsync(fileSaved);

            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == expectedFileName);
        }
        
        public async Task GetByFileName()
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

            var fileSaved = await this.Context.FileCollection<ImageBlob>().UploadAsync(file);

            Assert.True(fileSaved != ObjectId.Empty);

            var fileInfo = await this.Context.FileCollection<ImageBlob>().GetFileByNameAsync("image.jpg");

            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.FileName == "image.jpg");
        }
    }
}
