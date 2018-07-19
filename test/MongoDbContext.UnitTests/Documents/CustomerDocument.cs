using System.Collections.Generic;

namespace MongoDbFramework.UnitTests.Documents
{
    public class CustomerDocument : Document
    {
        public string Name { get; set; }

        public virtual List<OrderDocument> Orders { get; set; }
    }
}
