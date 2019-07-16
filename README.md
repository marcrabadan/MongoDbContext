# MongoDbContext
MongoDbContext enables .NET developers to work with a MongoDb database using .NET objects.

# How it works?

> Install-Package MongoDbContext

> Install-Package MongoDbContext.Abstractions

# 1 - Creating documents.

```csharp

    public class Tweet : IDocument<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Message { get; set; }
    }
    
    public class Movie : IDocument<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public int Minutes { get; set; }
    }
    
```

# 1 - Inherits from the MongoDbContext class.

**ModelBuilder for one mongodb node**
```csharp

    public class SocialContext : MongoDbContext
    {
        public SocialContext(MongoDbOptions<SocialContext> options) : base(options)
        {
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Document<Tweet>()
                .Map(c =>
                {
                    c.AutoMap();
                    c.SetDiscriminatorIsRequired(true);
                    c.SetDiscriminator(typeof(CustomerDocument).FullName);
                })
                .WithDatabase("socialDb")
                .WithCollection("tweets")
                .DefineIndex(c => c.Ascending(x => x.Name), c =>
                {
                    c.Name = "NameIndex";
                    c.Unique = true;
                });

            modelBuilder.Document<Movie>()
                .Map(c =>
                {
                    c.AutoMap();
                    c.SetDiscriminatorIsRequired(true);
                    c.SetDiscriminator(typeof(CustomerDocument).FullName);
                })
                .WithDatabase("socialDb")
                .WithCollection("movies")
                .DefineIndex(c => c.Ascending(x => x.Name), c =>
                {
                    c.Name = "NameIndex";
                    c.Unique = true;
                });
                            
            modelBuilder.Document<ImageBlob>()
                .WithDatabase("fileStorage")
                .AsFileStorage<ImageBlob>()
                .WithBucketName("ImageBlobBucket")
                .WithChunkSize(64512);
        }

        public MongoCollection<Tweet> Tweets { get; set; }

        public MongoCollection<Movie> Movies { get; set; }
        
        public MongoFileCollection<ImageBlob> Images { get; set; }
    }
    
```
**ModelBuilder for ReplicaSet**

```csharp

public class SocialContext : MongoDbContext
    {
        public SocialContext(MongoDbOptions<SocialContext> options) : base(options)
        {
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Document<Tweet>()
                .WithDatabase("social")
                .WithCollection("tweets")
                .WithDatabaseBehavior(c =>
                {
                    c.WithReadPreference(ReadPreference.SecondaryPreferred);
                    c.WithReadConcern(ReadConcern.Local);
                    c.WithWriteConcern(WriteConcern.WMajority);                    
                })
                .WithCollectionBehavior(c =>
                {
                    c.WithReadPreference(ReadPreference.SecondaryPreferred);
                    c.WithReadConcern(ReadConcern.Local);
                    c.WithWriteConcern(WriteConcern.WMajority);
                })
                .WithTransactionBehavior(c =>
                {
                    c.WithReadPreference(ReadPreference.Primary);
                    c.WithReadConcern(ReadConcern.Snapshot);
                    c.WithWriteConcern(WriteConcern.WMajority);
                })
                .WithSessionBehavior(c =>
                {
                    c.WithReadPreference(ReadPreference.Primary);
                });
        }

        public MongoCollection<Tweet> Tweets { get; set; }
    }

```

# 2 - Dependency Injection

```csharp

  var services = new ServiceCollection();
  services.AddMongoDbContext<SocialContext>();
    
```
**IoC Provider supported:**

> Install-Package MongoDbContext.Extensions.DependencyInjection

> Install-Package MongoDbContext.Autofac

> Install-Package MongoDbContext.CastleWindsor

# 3 - Configuring

In the example, SocialContext has a TRANSIENT lifestyle and your configuration has a SINGLETON lifestyle, both by default is SCOPED lifestyle.

You can choose by connection string or a custom configuration from MongoClientSettings.

Azure Cosmos Db configuration is enabled in 1.0.1.

**Configuration via Connection String:**

```csharp

  var services = new ServiceCollection();
services.AddMongoDbContext<SocialContext>(options =>
{
    //options.ConnectionString("mongodb://localhost:27017");
    options.ConnectionString("mongodb://<serviceName>:<PRIMARYPASSWORD>@<serviceName>.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
}, ServiceLifetime.Transient, ServiceLifetime.Singleton);
    
```
**ReplicaSet Configuration:**
You can see how it works with replicaset.  [https://github.com/marcrabadan/MongoReplicaSet]

```csharp

  var services = new ServiceCollection();
  services.AddMongoDbContext<SocialContext>(c =>
    {
        c.Configure(x =>
        {
            x.Credential = mongoCredential;
            x.Servers = new[]
            {
              new MongoServerAddress("mongo-rs-01", 27017),
              new MongoServerAddress("mongo-rs-02", 27017),
              new MongoServerAddress("mongo-rs-03", 27017)
            };                    
            x.ConnectionMode = ConnectionMode.ReplicaSet;
            x.ReplicaSetName = "rs0";
            x.ReadPreference = ReadPreference.Nearest;
            x.ReadConcern = ReadConcern.Snapshot;
            x.WriteConcern = WriteConcern.WMajority;
        });
}, ServiceLifetime.Transient, ServiceLifetime.Singleton);
    
```

# NEXT STEPS:
- Enable Sharding

# NOTE:

- Azure Cosmos Db only for CRUD Operations.
- Map/Reduce is not supported in Azure Cosmos Db.
