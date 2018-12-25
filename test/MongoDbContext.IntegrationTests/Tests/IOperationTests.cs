using MongoDbFramework.IntegrationTests.Enums;
using System.Threading.Tasks;

namespace MongoDbFramework.IntegrationTests
{
    public interface IOperationTests
    {
        Task ShouldAddItemToDatabase(IoCType ioCType);
        Task ShouldUpdateItemFromDatabase(IoCType ioCType);
        Task ShouldDeleteItemFromDatabase(IoCType ioCType);
        Task ShouldGetAllItemsFromDatabase(IoCType ioCType);
        Task ShouldGetItemsFromDatabase(IoCType ioCType);
        Task ShouldGetFirstOrDefaultItemFromDatabase(IoCType ioCType);
        Task ShouldAddRangeItemsToDatabase(IoCType ioCType);
        Task ShouldAddIndexAndRetrieveIt(IoCType ioCType);
        Task ShouldMapReduceOperation(IoCType ioCType);
    }
}
