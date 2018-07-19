using MongoDbFramework.IntegrationTests.Documents;
using System.Collections.Generic;

namespace MongoDbFramework.IntegrationTests.Mocks
{
    public static class MovieMock
    {
        public static List<Movie> GetMovieMocks()
        {
            return new List<Movie>
            {
                new Movie { Title="The Perfect Developer",
                    Category="SciFi", Minutes=118 },
                new Movie { Title="Lost In Frankfurt am Main",
                    Category="Horror", Minutes=122 },
                new Movie { Title="The Infinite Standup",
                    Category="Horror", Minutes=341 }
            };
        }
    }
}
