using MongoDbFramework.IntegrationTests.Enums;
using System.Threading.Tasks;

namespace MongoDbFramework.IntegrationTests.Tests
{
    public interface ITransactionOperationTests
    {
        Task ShouldCommitOperations(IoCType ioCType);
        Task ShouldRollbackOperations(IoCType ioCType);
    }
}
