using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDbFramework.Abstractions;
using MongoDbFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{

    public class MongoFileCollection<TFile> : IMongoFileCollection<TFile> where TFile : IFileDocument, new()
    {
        private readonly ConfigurationSource<TFile> configurationSource;

        public MongoFileCollection(ConfigurationSource<TFile> configurationSource)
        {
            this.configurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));

            Client = this.configurationSource?.ToMongoClient();
            Database = this.configurationSource?.ToMongoDatabase(Client);
            Bucket = this.configurationSource?.ToGridFsBucket(Database);
        }

        protected internal MongoClient Client { get; set; }
        protected internal IMongoDatabase Database { get; set; }
        protected internal GridFSBucket Bucket { get; set; }

        public Task<byte[]> DownloadByFileNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Bucket.DownloadAsBytesByNameAsync(fileName, null, cancellationToken);
        }

        public Task<byte[]> DownloadByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new InvalidOperationException("Id is null or empty.");

            return Bucket.DownloadAsBytesAsync(BsonValue.Create(id), null, cancellationToken);
        }

        public async Task<TFile> GetFileByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new InvalidOperationException("Id is null or empty.");
            var bsonDocument = typeof(TFile).FindKey().GetBsonValue(id);
            try
            {
                var find = await Bucket.FindAsync(bsonDocument, null, cancellationToken).ConfigureAwait(false);

                await find.MoveNextAsync(cancellationToken).ConfigureAwait(false);

                var fileInfo = find.Current.FirstOrDefault();
                if (fileInfo == null)
                    return default(TFile);

                var file = new TFile
                {
                    FileName = fileInfo.Filename
                };
                file.SetId(fileInfo.Id);
                SetMetadata(fileInfo, file);
                
                return file;
            }
            catch (Exception)
            {
                return default(TFile);
            }
        }

        public async Task<TFile> GetFileByNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var find = await Bucket.FindAsync(new BsonDocument("filename", fileName), null, cancellationToken).ConfigureAwait(false);
           
            await find.MoveNextAsync(cancellationToken).ConfigureAwait(false);

            var fileInfo = find.Current.FirstOrDefault();
            if (fileInfo == null)
                return default(TFile);
            
            var file = new TFile
            {
                FileName = fileInfo.Filename
            };
            file.SetId(fileInfo.Id);

            SetMetadata(fileInfo, file);

            return file;
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
            var files = await Bucket.FindAsync(filter, options, cancellationToken).ConfigureAwait(false);
            while (await files.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                var cursor = files.Current.Select(gridFsFileInfo =>
                {
                    var file = new TFile
                    {
                        FileName = gridFsFileInfo.Filename
                    };
                    file.SetId(gridFsFileInfo.Id);
                    SetMetadata(gridFsFileInfo, file);

                    return file;
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
                ChunkSizeBytes = this.configurationSource?.Model?.FileStorageOptions?.ChunkSize ?? 1048576,
                Metadata = MergeMetadata(this.configurationSource?.Model?.FileStorageOptions?.MetaData, file).ToBsonDocument()
            }, cancellationToken).ConfigureAwait(false);

            return id;
        }

        public async Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new InvalidOperationException("Id is null or empty.");
            
            await Bucket.DeleteAsync(BsonValue.Create(id), cancellationToken).ConfigureAwait(false);
        }

        public async Task RenameAsync<TKey>(TKey id, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new InvalidOperationException("Id is null or empty.");

            await Bucket.RenameAsync(BsonValue.Create(id), fileName, cancellationToken).ConfigureAwait(false);
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

        private void SetMetadata(GridFSFileInfo fileInfo, TFile file)
        {
            string fileType = string.Empty;
            var dictionary = fileInfo.Metadata?.ToDictionary();
            if (dictionary != null)
            {
                fileType = dictionary.ContainsKey("FileType") ? dictionary["FileType"].ToString() : string.Empty;
            }
            
            file.FileType = fileType;
            file.Metadata = MergeMetadata(fileInfo, file);
        }
        
        private Dictionary<string, object> MergeMetadata(Dictionary<string, object> metadataConfig, TFile file)
        {
            var result = new Dictionary<string, object>();
            file.Metadata = file.Metadata ?? new Dictionary<string, object>();
            metadataConfig = metadataConfig ?? new Dictionary<string, object>();

            if (!(file.Metadata.Any() && metadataConfig.Any()))
                return null;

            foreach (var fileMetadata in file.Metadata)
            {
                if (!result.ContainsKey(fileMetadata.Key))
                {
                    result.Add(fileMetadata.Key, fileMetadata.Value);
                }
            }

            foreach (var fileMetadata in metadataConfig)
            {
                if (!result.ContainsKey(fileMetadata.Key))
                {
                    result.Add(fileMetadata.Key, fileMetadata.Value);
                }
                else
                {
                    result.Add(fileMetadata.Key, fileMetadata.Value ?? result[fileMetadata.Key]);
                }
            }

            return result;
        }

        private Dictionary<string, object> MergeMetadata(GridFSFileInfo fileInfo, TFile file)
        {
            var result = new Dictionary<string, object>();
            file.Metadata = file.Metadata ?? new Dictionary<string, object>();
            var dict = fileInfo?.Metadata?.ToDictionary() ?? new Dictionary<string, object>();

            if (!(file.Metadata.Any() && dict.Any()))
                return null;

            foreach (var fileMetadata in file.Metadata)
            {
                if (!result.ContainsKey(fileMetadata.Key))
                {
                    result.Add(fileMetadata.Key, fileMetadata.Value);
                }
            }

            foreach (var fileMetadata in dict)
            {
                if (!result.ContainsKey(fileMetadata.Key))
                {
                    result.Add(fileMetadata.Key, fileMetadata.Value);
                }
                else
                {
                    result.Add(fileMetadata.Key, fileMetadata.Value ?? result[fileMetadata.Key]);
                }
            }

            return result;
        }

        public void Dispose()
        {
            this.configurationSource?.Dispose();
            this.Client = null;
            this.Database = null;
            this.Bucket = null;
        }
    }
}
