using MongoDB.Bson.Serialization.Attributes;

namespace MongoDbFramework
{
    public interface IDocument
    {
    }

    public interface IDocument<TKey> : IDocument where TKey : struct
    {
        [BsonId]
        TKey Id { get; set; }
    }
}
