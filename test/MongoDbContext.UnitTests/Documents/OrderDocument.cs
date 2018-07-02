using MongoDbContext.Documents;

namespace MongoDbContext.UnitTests.Documents
{
    public class OrderDocument : Document
    {
        public virtual CustomerDocument Customer { get; set; }
    }
}
