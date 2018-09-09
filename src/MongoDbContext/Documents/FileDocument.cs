using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDbFramework
{
    public class FileDocument : IDocument<ObjectId>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public byte[] Data { get; set; }

        public Dictionary<string, object> Metadata { get; set; }
    }
}
