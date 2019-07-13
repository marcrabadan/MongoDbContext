namespace MongoDbFramework.Abstractions
{
    public interface IDocument
    {
    }

    public interface IDocument<TKey> : IDocument
    {
        TKey Id { get; set; }
    }
}
