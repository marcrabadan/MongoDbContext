using MongoDbFramework.Abstractions;
using System;

namespace MongoDbFramework.UnitTests.Documents
{
    public class OrderDocument : IDocument<Guid>
    {
        public Guid Id { get; set; }
        public virtual CustomerDocument Customer { get; set; }
    }
}
