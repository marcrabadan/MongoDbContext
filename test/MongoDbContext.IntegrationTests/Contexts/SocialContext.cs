using MongoDbFramework.IntegrationTests.Documents;

namespace MongoDbFramework.IntegrationTests.Contexts
{
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
                .WithCollection("movies")
                .DefineIndex(c => c.Ascending(x => x.Category), c =>
                {
                    c.Name = "CategoryIndex";
                    c.Unique = false;
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
}
