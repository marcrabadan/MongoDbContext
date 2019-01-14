using MongoDB.Driver;
using MongoDbFramework.UnitTests.Documents;
using System.Linq;
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
                .Map(c =>
                {
                    c.AutoMap();
                    c.SetDiscriminatorIsRequired(true);
                    c.SetDiscriminator(typeof(CustomerDocument).FullName);
                })
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
                .Map(c =>
                {
                    c.AutoMap();
                    c.SetDiscriminatorIsRequired(true);
                    c.SetDiscriminator(typeof(CustomerDocument).FullName);
                })
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

        [Fact]
        public void ApplyDatabaseBehavior()
        {
            var mongoClient = new MongoClient();
            var modelBuilder = new ModelBuilder(mongoClient);
            var readPreferenceExpected = ReadPreference.Primary;
            var readConcernExpected = ReadConcern.Majority;
            var writeConcernExpected = WriteConcern.WMajority;
            
            modelBuilder
                .Document<CustomerDocument>()
                .WithDatabaseBehavior(c =>
                {
                    c.WithReadPreference(ReadPreference.Primary);
                    c.WithReadConcern(ReadConcern.Majority);
                    c.WithWriteConcern(WriteConcern.WMajority);
                });

            Assert.True(modelBuilder.Models.ContainsKey(typeof(CustomerDocument)));
            Assert.True(modelBuilder.Models.TryGetValue(typeof(CustomerDocument), out var configurationSource));
            Assert.True(configurationSource is ConfigurationSource<CustomerDocument>);
            var configurationSourceTyped = (ConfigurationSource<CustomerDocument>)configurationSource;

            var databaseBehavior = configurationSourceTyped.Model.DatabaseBehavior;
            Assert.True(databaseBehavior.ReadPreference == readPreferenceExpected);
            Assert.True(databaseBehavior.ReadConcern == readConcernExpected);
            Assert.True(databaseBehavior.WriteConcern == writeConcernExpected);
        }

        [Fact]
        public void ApplyTransactionBehavior()
        {
            var mongoClient = new MongoClient();
            var modelBuilder = new ModelBuilder(mongoClient);
            var readPreferenceExpected = ReadPreference.Primary;
            var readConcernExpected = ReadConcern.Majority;
            var writeConcernExpected = WriteConcern.WMajority;

            modelBuilder
                .Document<CustomerDocument>()
                .WithTransactionBehavior(c =>
                {
                    c.WithReadPreference(ReadPreference.Primary);
                    c.WithReadConcern(ReadConcern.Majority);
                    c.WithWriteConcern(WriteConcern.WMajority);
                });

            Assert.True(modelBuilder.Models.ContainsKey(typeof(CustomerDocument)));
            Assert.True(modelBuilder.Models.TryGetValue(typeof(CustomerDocument), out var configurationSource));
            Assert.True(configurationSource is ConfigurationSource<CustomerDocument>);
            var configurationSourceTyped = (ConfigurationSource<CustomerDocument>)configurationSource;

            var transactionBehavior = configurationSourceTyped.Model.TransactionBehavior;
            Assert.True(transactionBehavior.ReadPreference == readPreferenceExpected);
            Assert.True(transactionBehavior.ReadConcern == readConcernExpected);
            Assert.True(transactionBehavior.WriteConcern == writeConcernExpected);
        }

        [Fact]
        public void ApplySessionBehavior()
        {
            var mongoClient = new MongoClient();
            var modelBuilder = new ModelBuilder(mongoClient);
            var readPreferenceExpected = ReadPreference.Primary;
            var readConcernExpected = ReadConcern.Majority;
            var writeConcernExpected = WriteConcern.WMajority;

            modelBuilder
                .Document<CustomerDocument>()
                .WithSessionBehavior(c =>
                {
                    c.EnableCasualConsitency();
                    c.WithReadPreference(ReadPreference.Primary);
                    c.WithReadConcern(ReadConcern.Majority);
                    c.WithWriteConcern(WriteConcern.WMajority);
                });

            Assert.True(modelBuilder.Models.ContainsKey(typeof(CustomerDocument)));
            Assert.True(modelBuilder.Models.TryGetValue(typeof(CustomerDocument), out var configurationSource));
            Assert.True(configurationSource is ConfigurationSource<CustomerDocument>);
            var configurationSourceTyped = (ConfigurationSource<CustomerDocument>)configurationSource;

            var sessionBehavior = configurationSourceTyped.Model.SessionBehavior;
            Assert.True(sessionBehavior.ReadPreference == readPreferenceExpected);
            Assert.True(sessionBehavior.ReadConcern == readConcernExpected);
            Assert.True(sessionBehavior.WriteConcern == writeConcernExpected);
        }
    }
}
