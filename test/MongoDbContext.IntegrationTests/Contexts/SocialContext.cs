using MongoDbContext.IntegrationTests.Documents;
using MongoDbContext.Options;

namespace MongoDbContext.IntegrationTests.Contexts
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
                .WithCollection("movies");
        }

        public MongoCollection<Tweet> Tweets { get; set; }

        public MongoCollection<Movie> Movies { get; set; }
    }
}
