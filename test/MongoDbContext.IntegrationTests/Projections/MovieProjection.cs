using MongoDbContext.Documents;

namespace MongoDbContext.IntegrationTests.Projections
{
    public class MovieProjection
    {
        public int Count { get; set; }
        public int TotalMinutes { get; set; }
        public double Average { get; set; }
    }
}
