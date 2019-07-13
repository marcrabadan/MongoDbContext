using MongoDB.Driver;
using MongoDbFramework.Abstractions;
using System;
using System.Collections.Generic;

namespace MongoDbFramework
{
    public sealed class FileStorageOptions<T> : FileStorageOptions where T : IFileDocument
    {
        public Type Type => typeof(T);
    }

    public class FileStorageOptions
    {
        public string BucketName { get; set; }
        public int ChunkSize { get; set; }
        public ReadConcern ReadConcern { get; set; }
        public ReadPreference ReadPreference { get; set; }
        public WriteConcern WriteConcern { get; set; }
        public Dictionary<string, object> MetaData { get; set; }
    }
}
