using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDbFramework.Documents;

namespace MongoDbFramework
{
    public interface IMongoFileCollection<TFile> where TFile : FileDocument, new()
    {
        Task<byte[]> DownloadByFileNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));
        Task<byte[]> DownloadByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TFile> GetFileByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
        Task<TFile> GetFileByNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<TFile>> GetFilesAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<string> UploadAsync(TFile file, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
        Task RenameAsync(string id, string fileName, CancellationToken cancellationToken = default(CancellationToken));
    }
}
