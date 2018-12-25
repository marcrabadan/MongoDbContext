# MongoDbContext
MongoDbContext enables .NET developers to work with a MongoDb database using .NET objects.

# How it works?

> Install-Package MongoDbContext

# 1 - Creating documents.

```csharp

    public class Tweet :  Document
    {
        public string Message { get; set; }
    }
    
    public class Movie : Document
    { 
        public string Title { get; set; }
        public string Category { get; set; }
        public int Minutes { get; set; }
    }
    
```

# 1 - Inherits from the MongoDbContext class.

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

# 2 - Dependency Injection

```csharp

  var services = new ServiceCollection();
  services.AddMongoDbContext<SocialContext>();
    
```
## IoC Provider supported:

> Install-Package MongoDbContext.Extensions.DependencyInjection

> Install-Package MongoDbContext.Autofac

> Install-Package MongoDbContext.CastleWindsor

# 3 - Configuring

SocialContext has a TRANSIENT lifestyle and your configuration has a SINGLETON lifestyle, both by default is SCOPED lifestyle.

You can choose by connection string or a custom configuration from MongoClientSettings.

Azure Cosmos Db configuration is enabled in 1.0.1.

```csharp

  var services = new ServiceCollection();
services.AddMongoDbContext<SocialContext>(options =>
{
    //options.ConnectionString("mongodb://localhost:27017");
    options.ConnectionString("mongodb://<serviceName>:<PRIMARYPASSWORD>@<serviceName>.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
}, ServiceLifetime.Transient, ServiceLifetime.Singleton);
    
```

# NOTE:

- Azure Cosmos Db only for CRUD Operations.
- Map/Reduce is not supported in Azure Cosmos Db.
