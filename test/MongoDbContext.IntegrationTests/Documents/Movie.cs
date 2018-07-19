namespace MongoDbFramework.IntegrationTests.Documents
{
    public class Movie : Document
    { 
        public string Title { get; set; }
        public string Category { get; set; }
        public int Minutes { get; set; }
    }
}
