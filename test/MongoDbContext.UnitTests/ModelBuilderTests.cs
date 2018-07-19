using System.Linq;
using MongoDB.Driver;
using MongoDbFramework.UnitTests.Documents;
using Xunit;

namespace MongoDbFramework.UnitTests
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

        [Fact]
        public void ApplySingleIndex()
        {
            var mongoClient = new MongoClient();
            var modelBuilder = new ModelBuilder(mongoClient);
            var expected = new IndexKeysDefinitionBuilder<CustomerDocument>();
            expected.Ascending(c => c.Name);

            modelBuilder
                .Document<CustomerDocument>()
                .DefineIndex(c => c.Ascending(x => x.Name), c =>
                {
                    c.Name = "Index1";
                    c.Unique = true;
                });

            Assert.True(modelBuilder.Models.ContainsKey(typeof(CustomerDocument)));
            Assert.True(modelBuilder.Models.TryGetValue(typeof(CustomerDocument), out var configurationSource));
            Assert.True(configurationSource is ConfigurationSource<CustomerDocument>);
            var configurationSourceTyped = (ConfigurationSource<CustomerDocument>)configurationSource;
            Assert.True(configurationSourceTyped.Model.Indices.Any());
        }

        [Fact]
        public void ApplyMultipleIndex()
        {
            var mongoClient = new MongoClient();
            var modelBuilder = new ModelBuilder(mongoClient);
            var expected = new IndexKeysDefinitionBuilder<CustomerDocument>();
            expected.Ascending(c => c.Name);

            modelBuilder
                .Document<CustomerDocument>()
                .DefineIndex(c => c.Ascending(x => x.Name), c =>
                {
                    c.Name = "Index1";
                    c.Unique = true;
                })
                .DefineIndex(c => c.Ascending(x => x.Name), c =>
                {
                    c.Name = "Index2";
                    c.Unique = true;
                });

            Assert.True(modelBuilder.Models.ContainsKey(typeof(CustomerDocument)));
            Assert.True(modelBuilder.Models.TryGetValue(typeof(CustomerDocument), out var configurationSource));
            Assert.True(configurationSource is ConfigurationSource<CustomerDocument>);
            var configurationSourceTyped = (ConfigurationSource<CustomerDocument>)configurationSource;
            Assert.True(configurationSourceTyped.Model.Indices.Any());
            Assert.True(configurationSourceTyped.Model.Indices.Count == 2);
        }
    }
}
