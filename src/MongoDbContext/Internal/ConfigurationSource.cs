using MongoDB.Driver;
using System;

namespace MongoDbFramework
{
    public sealed class ConfigurationSource<TDocument> : ConfigurationSource where TDocument : IDocument
    {
        public ConfigurationSource(MongoClient client) : base(client)
        {
        }

        internal Model<TDocument> Model { get; set; }

        public override void DisposeResources()
        {
            this.Model = null;
        }
    }

    public abstract class ConfigurationSource : IDisposable
    {
        public ConfigurationSource(MongoClient client)
        {
            Source = client;
        }

        internal MongoClient Source { get; set; }

        public abstract void DisposeResources();

        public void Dispose()
        {
            this.Source = null;
            this.DisposeResources();
        }
    }
}
