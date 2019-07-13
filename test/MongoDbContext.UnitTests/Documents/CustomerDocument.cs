using MongoDbFramework.Abstractions;
using System;
using System.Collections.Generic;

namespace MongoDbFramework.UnitTests.Documents
{
    public class CustomerDocument : IDocument<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual List<OrderDocument> Orders { get; set; }
    }
}
