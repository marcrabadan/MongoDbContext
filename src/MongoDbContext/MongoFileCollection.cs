using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDbFramework.Documents;
using MongoDbFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongoDbFramework
{
    public class MongoFileCollection<TFile> : IMongoFileCollection<TFile> where TFile : FileDocument, new()
    {
        public MongoFileCollection(ConfigurationSource<TFile> configurationSource)
        {
            ConfigurationSource = configurationSource;
            Client = configurationSource.ToMongoClient();
            Database = configurationSource.ToMongoDatabase(Client);
            Bucket = configurationSource.ToGridFsBucket(Database);
        }

        internal MongoClient Client { get; }
        internal IMongoDatabase Database { get; }
        internal GridFSBucket Bucket { get; }
        internal ConfigurationSource<TFile> ConfigurationSource { get; }

        public async Task<byte[]> DownloadByFileNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Bucket.DownloadAsBytesByNameAsync(fileName, null, cancellationToken);
        }

        public async Task<byte[]> DownloadByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(!ObjectId.TryParse(id, out var objectId)) throw new InvalidOperationException($"this id '{id}' is invalid.");

            return await Bucket.DownloadAsBytesAsync(objectId, null, cancellationToken);
        }

        public async Task<TFile> GetFileByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(string.IsNullOrEmpty(id)) throw new InvalidOperationException("Id is null or empty.");
            if(!ObjectId.TryParse(id, out var objectId)) throw new InvalidOperationException("Id is not ObjectId.");

            try
            {
                var find = await Bucket.FindAsync(new BsonDocument("_id", objectId), null, cancellationToken);

                await find.MoveNextAsync(cancellationToken);

                var file = find.Current.FirstOrDefault();
                if (file == null)
                    return default(TFile);

                return new TFile
                {
                    Id = file.Id,
                    FileName = file.Filename,
                    Length = file.Length,
                    UploadDateTime = file.UploadDateTime,
                    Metadata = file.Metadata?.ToDictionary()
                };
            }
            catch (Exception e)
            {
                return default(TFile);
            }
        }

        public async Task<TFile> GetFileByNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var find = await Bucket.FindAsync(new BsonDocument("filename", fileName), null, cancellationToken);

            await find.MoveNextAsync(cancellationToken);

            var file = find.Current.FirstOrDefault();
            if (file == null)
                return default(TFile);

            return new TFile
            {
                Id = file.Id,
                FileName = file.Filename,
                Length = file.Length,
                UploadDateTime = file.UploadDateTime,
                Metadata = file.Metadata?.ToDictionary()
            };
        }

        public async Task<List<TFile>> GetFilesAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var list = new List<TFile>();
            var filter = Builders<GridFSFileInfo>.Filter.Empty;
            var options = new GridFSFindOptions
            {
                BatchSize = 100,
                Limit = 100
            };
            var files = await Bucket.FindAsync(filter, options, cancellationToken);
            while (await files.MoveNextAsync(cancellationToken))
            {
                var cursor = files.Current.Select(c => new TFile
                {
                    Id = c.Id,
                    FileName = c.Filename,
                    Length = c.Length,
                    UploadDateTime = c.UploadDateTime,
                    Metadata = c.Metadata?.ToDictionary()
                }).ToList();

                list.AddRange(cursor);
            }

            return list;
        }

        public async Task<string> UploadAsync(TFile file, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(file.FileName))
                throw new InvalidOperationException("FileName is empty.");

            if (file.Data == default(byte[]))
                throw new InvalidOperationException("File data is empty.");

            var id = await Bucket.UploadFromBytesAsync(file.FileName, file.Data, new GridFSUploadOptions
            {
                ChunkSizeBytes = ConfigurationSource?.Model?.FileStorageOptions?.ChunkSize ?? 1048576,
                Metadata = ConfigurationSource?.Model?.FileStorageOptions?.MetaData?.ToBsonDocument() ?? null
            }, cancellationToken);

            return id.ToString();
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id)) throw new InvalidOperationException("Id is null or empty.");
            if (!ObjectId.TryParse(id, out var objectId)) throw new InvalidOperationException("Id is not ObjectId.");
            
            await Bucket.DeleteAsync(objectId, cancellationToken);
        }

        public async Task RenameAsync(string id, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id)) throw new InvalidOperationException("Id is null or empty.");
            if (!ObjectId.TryParse(id, out var objectId)) throw new InvalidOperationException("Id is not ObjectId.");
            
            await Bucket.RenameAsync(objectId, fileName, cancellationToken);
        }
    }
}
