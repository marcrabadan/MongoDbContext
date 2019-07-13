using MongoDbFramework.Abstractions;
using MongoDbFramework.Attributes;
using System;

namespace MongoDbFramework.IntegrationTests.Documents
{
    public class Tweet : IDocument<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Message { get; set; }
    }
}
