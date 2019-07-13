using MongoDbFramework.Abstractions;
using MongoDbFramework.Attributes;
using System;

namespace MongoDbFramework.IntegrationTests.Documents
{
    public class Movie : IDocument<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public int Minutes { get; set; }
    }
}
