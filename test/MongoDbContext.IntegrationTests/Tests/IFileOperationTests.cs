using MongoDbFramework.IntegrationTests.Enums;
using System.Threading.Tasks;

namespace MongoDbFramework.IntegrationTests.Tests
{
    public interface IFileOperationTests
    {
        Task ShouldUploadAndDeleteFile(IoCType ioCType);
        Task ShouldUploadAndGetFileById(IoCType ioCType);
        Task ShouldGetAllFiles(IoCType ioCType);
        Task ShouldRenameFile(IoCType ioCType);
        Task ShouldGetByFileName(IoCType ioCType);
    }
}
