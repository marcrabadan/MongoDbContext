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

# 1 - Inherits from the ElasticSearchContext class.

```csharp

    public class SocialContext : MongoDbContext
    {
        public SocialContext(MongoDbOptions<SocialContext> options) : base(options)
        {
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Document<Tweet>()
                .WithDatabase("socialDb")
                .WithCollection("tweets");

            modelBuilder.Document<Movie>()
                .WithDatabase("socialDb")
                .WithCollection("movies");
        }

        public MongoCollection<Tweet> Tweets { get; set; }

        public MongoCollection<Movie> Movies { get; set; }
    }
    
```

# 2 - Dependency Injection

```csharp

  var services = new ServiceCollection();
  services.AddMongoDbContext<SocialContext>();
    
```

# 3 - Configuring

SocialContext has a TRANSIENT lifestyle and your configuration has a SINGLETON lifestyle, both by default is SCOPED lifestyle.

You can choose by connection string or a custom configuration from MongoClientSettings.

Azure Cosmos Db configuration is enabled in 1.0.1.

```csharp

  var services = new ServiceCollection();
services.AddMongoDbContext<TContext>(options =>
{
    //options.ConnectionString("mongodb://localhost:27017");
    options.ConnectionString("mongodb://<serviceName>:<PRIMARYPASSWORD>@<serviceName>.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
}, ServiceLifetime.Transient, ServiceLifetime.Singleton);
    
```

# NOTE:

- Azure Cosmos Db only for CRUD Operations.
- Map/Reduce is not supported in Azure Cosmos Db.
