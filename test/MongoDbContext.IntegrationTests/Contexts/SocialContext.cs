using MongoDB.Driver;
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
            modelBuilder
                .Document<Tweet>()
                .Map(c =>
                {
                    c.AutoMap();
                    c.SetDiscriminatorIsRequired(true);
                    c.SetDiscriminator(typeof(Tweet).FullName);
                })
                .WithDatabase("socialDb")
                .WithCollection("tweets");

            modelBuilder
                .Document<Movie>()
                .Map(c =>
                {
                    c.AutoMap();
                    c.SetDiscriminatorIsRequired(true);
                    c.SetDiscriminator(typeof(Movie).FullName);
                })
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
