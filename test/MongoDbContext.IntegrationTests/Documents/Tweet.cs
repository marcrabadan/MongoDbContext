using MongoDbContext.Documents;

namespace MongoDbContext.IntegrationTests.Documents
{
    public class Tweet :  Document
    {
        public string Message { get; set; }
    }
}
