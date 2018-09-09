using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace MongoDbFramework
{
    public class FileDocumentTypeBuilder<T> where T : FileDocument
    {
        private Action<FileDocumentTypeBuilder<T>> _apply;

        public FileDocumentTypeBuilder(Action<FileDocumentTypeBuilder<T>> apply)
        {
            _apply = apply;
            MetaData = new Dictionary<string, object>();
        }

        internal string BucketName { get; set; }
        internal int ChunkSize { get; set; }
        internal ReadConcern ReadConcern { get; set; }
        internal ReadPreference ReadPreference { get; set; }
        internal WriteConcern WriteConcern { get; set; }
        internal Dictionary<string, object> MetaData { get; set; }

        public FileDocumentTypeBuilder<T> WithBucketName(string bucketName)
        {
            BucketName = bucketName;
            _apply(this);
            return this;
        }

        public FileDocumentTypeBuilder<T> WithChunkSize(int chunkSize)
        {
            if(chunkSize <= 0)
                throw new InvalidOperationException("ChunkSize should be greater than 0.");

            ChunkSize = chunkSize;
            _apply(this);
            return this;
        }

        public FileDocumentTypeBuilder<T> WithReadConcern(ReadConcern readConcern)
        {
            ReadConcern = readConcern;
            _apply(this);
            return this;
        }

        public FileDocumentTypeBuilder<T> WithReadPreference(ReadPreference readPreference)
        {
            ReadPreference = readPreference;
            _apply(this);
            return this;
        }

        public FileDocumentTypeBuilder<T> WithWriteConcern(WriteConcern writeConcern)
        {
            WriteConcern = writeConcern;
            _apply(this);
            return this;
        }
        
        public FileDocumentTypeBuilder<T> AddMetadata(string key, object value)
        {
            if (MetaData.ContainsKey(key))
                throw new InvalidOperationException($"This '{key}' metadata key exists.");

            MetaData.Add(key, value);
            _apply(this);
            return this;
        }

        internal FileStorageOptions<T> Build() => new FileStorageOptions<T>
        {
            BucketName = BucketName,
            ChunkSize = ChunkSize,
            ReadConcern = ReadConcern,
            ReadPreference = ReadPreference,
            WriteConcern = WriteConcern,
            MetaData = MetaData
        };
    }
}
