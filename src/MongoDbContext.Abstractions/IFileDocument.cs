using MongoDbFramework.Abstractions;
using System.Collections.Generic;

namespace MongoDbFramework.Abstractions
{
    public interface IFileDocument : IDocument
    {
        string FileName { get; set; }
        string FileType { get; set; }
        byte[] Data { get; set; }
        Dictionary<string, object> Metadata { get; set; }
    }

    public interface IFileDocument<TKey> : IFileDocument
    {
        TKey Id { get; set; }
    }
}
