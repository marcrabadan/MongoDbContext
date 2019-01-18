using MongoDB.Driver;
using System.Text;

namespace MongoDbFramework
{
    public class Behavior
    {
        public ReadPreference ReadPreference { get; set; }
        public ReadConcern ReadConcern { get; set; }
        public Encoding ReadEncoding { get; set; }
        public WriteConcern WriteConcern { get; set; }
        public Encoding WriteEncoding { get; set; }
    }
}
