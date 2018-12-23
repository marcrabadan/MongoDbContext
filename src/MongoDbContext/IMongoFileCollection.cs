using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{
    public interface IMongoFileCollection<TFile> : IDisposable where TFile : FileDocument, new()
    {
        Task<byte[]> DownloadByFileNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));
        Task<byte[]> DownloadByIdAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TFile> GetFileByIdAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TFile> GetFileByNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TFile>> GetFilesAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<ObjectId> UploadAsync(TFile file, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(ObjectId id, CancellationToken cancellationToken = default(CancellationToken));
        Task RenameAsync(ObjectId id, string fileName, CancellationToken cancellationToken = default(CancellationToken));
    }
}
