using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<byte[]> DownloadByIdAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null || id == ObjectId.Empty) throw new InvalidOperationException("Id is null or empty.");

            return await Bucket.DownloadAsBytesAsync(id, null, cancellationToken);
        }

        public async Task<TFile> GetFileByIdAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null || id == ObjectId.Empty) throw new InvalidOperationException("Id is null or empty.");

            try
            {
                var find = await Bucket.FindAsync(new BsonDocument("_id", id), null, cancellationToken);

                await find.MoveNextAsync(cancellationToken);

                var file = find.Current.FirstOrDefault();
                if (file == null)
                    return default(TFile);

                return new TFile
                {
                    Id = file.Id,
                    FileName = file.Filename,
                    Metadata = file.Metadata?.ToDictionary()
                };
            }
            catch (Exception)
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
                    Metadata = c.Metadata?.ToDictionary()
                }).ToList();

                list.AddRange(cursor);
            }

            return list;
        }

        public async Task<ObjectId> UploadAsync(TFile file, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(file.FileName))
                throw new InvalidOperationException("FileName is empty.");

            if (file.Data == default(byte[]))
                throw new InvalidOperationException("File data is empty.");

            if(file.Metadata == null)
                file.Metadata = new Dictionary<string, object>();

            SetDefaultValues(State.Created, file);

            var id = await Bucket.UploadFromBytesAsync(file.FileName, file.Data, new GridFSUploadOptions
            {
                ChunkSizeBytes = ConfigurationSource?.Model?.FileStorageOptions?.ChunkSize ?? 1048576,
                Metadata = ConfigurationSource?.Model?.FileStorageOptions?.MetaData?.ToBsonDocument()
            }, cancellationToken);

            return id;
        }

        public async Task DeleteAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null || id == ObjectId.Empty) throw new InvalidOperationException("Id is null or empty.");
            
            await Bucket.DeleteAsync(id, cancellationToken);
        }

        public async Task RenameAsync(ObjectId id, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null || id == ObjectId.Empty) throw new InvalidOperationException("Id is null or empty.");

            await Bucket.RenameAsync(id, fileName, cancellationToken);
        }

        private void SetDefaultValues(State state, TFile file)
        {
            foreach (var name in Enum.GetNames(typeof(FileDocumentMetadata)))
            {
                if (!file.Metadata.ContainsKey(name))
                {
                    switch (name)
                    {
                        case "FileType":
                            file.Metadata.Add("FileType", file.FileType);
                            break;
                        case "Length":
                            file.Metadata.Add("Length", file.Data?.Length ?? 0);
                            break;
                        case "CreatedAt":
                            if(state == State.Created) file.Metadata.Add("CreatedAt", DateTime.Now);
                            break;
                        case "ModifiedAt":
                            file.Metadata.Add("ModifiedAt", DateTime.Now);
                            break;
                    }
                }
                else
                {
                    switch (name)
                    {
                        case "FileType":
                            file.Metadata["FileType"] = file.FileType;
                            break;
                        case "Length":
                            file.Metadata["Length"] = file.Data?.Length ?? 0;
                            break;
                        case "CreatedAt":
                            if (state == State.Created) file.Metadata["CreatedAt"] = DateTime.Now;
                            break;
                        case "ModifiedAt":
                            file.Metadata["ModifiedAt"] = DateTime.Now;
                            break;
                    }
                }
            }
        }
    }
}
