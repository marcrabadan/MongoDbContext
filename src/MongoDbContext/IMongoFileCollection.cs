using MongoDB.Bson;
using MongoDbFramework.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbFramework
{
    public interface IMongoFileCollection<TFile> : IDisposable where TFile : IFileDocument, new()
    {
        Task<byte[]> DownloadByFileNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));
        Task<byte[]> DownloadByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TFile> GetFileByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TFile> GetFileByNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TFile>> GetFilesAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<ObjectId> UploadAsync(TFile file, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync<TKey>(TKey id, CancellationToken cancellationToken = default(CancellationToken));
        Task RenameAsync<TKey>(TKey id, string fileName, CancellationToken cancellationToken = default(CancellationToken));
    }
}
