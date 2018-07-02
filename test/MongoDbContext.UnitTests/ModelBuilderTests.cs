using System;
using MongoDbContext.Internal;
using MongoDbContext.UnitTests.Documents;
using MongoDB.Driver;
using Xunit;

namespace MongoDbContext.UnitTests
{
    public class ModelBuilderTests
    {
        [Fact]
        public void ApplyDatabaseName()
        {
            var mongoClient = new MongoClient();
            var modelBuilder = new ModelBuilder(mongoClient);

            modelBuilder
                .Document<CustomerDocument>()
                .WithDatabase("Database");

            Assert.True(modelBuilder.Models.ContainsKey(typeof(CustomerDocument)));
            Assert.True(modelBuilder.Models.TryGetValue(typeof(CustomerDocument), out var configurationSource));
            Assert.True(configurationSource is ConfigurationSource<CustomerDocument>);
            var configurationSourceTyped = (ConfigurationSource<CustomerDocument>) configurationSource;
            Assert.True(configurationSourceTyped.Model.DatabaseName == "Database");
        }

        [Fact]
        public void ApplyDatabaseAndCollectionName()
        {
            var mongoClient = new MongoClient();
            var modelBuilder = new ModelBuilder(mongoClient);

            modelBuilder
                .Document<CustomerDocument>()
                .WithDatabase("Database")
                .WithCollection("Collection");

            Assert.True(modelBuilder.Models.ContainsKey(typeof(CustomerDocument)));
            Assert.True(modelBuilder.Models.TryGetValue(typeof(CustomerDocument), out var configurationSource));
            Assert.True(configurationSource is ConfigurationSource<CustomerDocument>);
            var configurationSourceTyped = (ConfigurationSource<CustomerDocument>)configurationSource;
            Assert.True(configurationSourceTyped.Model.DatabaseName == "Database");
            Assert.True(configurationSourceTyped.Model.CollectionName == "Collection");
        }
    }
}
