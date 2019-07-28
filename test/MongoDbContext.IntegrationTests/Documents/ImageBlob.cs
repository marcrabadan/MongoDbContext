using MongoDB.Bson;
using MongoDbFramework.Abstractions;
using System.Collections.Generic;

namespace MongoDbFramework.IntegrationTests.Documents
{
    public class ImageBlob : IFileDocument<ObjectId>
    {
        [Key]
        public ObjectId Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] Data { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
