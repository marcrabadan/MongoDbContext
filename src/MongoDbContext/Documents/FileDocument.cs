using System;
using System.Collections.Generic;

namespace MongoDbFramework.Documents
{
    public class FileDocument : Document
    {
        public string FileName { get; set; }

        public long Length { get; set; }

        public byte[] Data { get; set; }

        public Dictionary<string, object> Metadata { get; set; }

        public DateTime UploadDateTime { get; set; }
    }
}
