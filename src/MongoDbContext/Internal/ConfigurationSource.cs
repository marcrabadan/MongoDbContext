using MongoDB.Driver;

namespace MongoDbFramework
{
    public class ConfigurationSource<TDocument> : ConfigurationSource where TDocument : Document
    {
        public ConfigurationSource(MongoClient client) : base(client)
        {
        }

        internal Model<TDocument> Model { get; set; }
    }

    public class ConfigurationSource
    {
        public ConfigurationSource(MongoClient client)
        {
            Source = client;
        }

        internal MongoClient Source { get; set; }
    }
}
