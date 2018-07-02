using System.Collections.Generic;
using MongoDbContext.Documents;

namespace MongoDbContext.UnitTests.Documents
{
    public class CustomerDocument : Document
    {
        public string Name { get; set; }

        public virtual List<OrderDocument> Orders { get; set; }
    }
}
