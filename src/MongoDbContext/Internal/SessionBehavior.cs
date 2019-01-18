using System.Text;
using MongoDB.Driver;

namespace MongoDbFramework
{
    public class SessionBehavior
    {
        public bool CasualConsistency { get; set; }
        public ReadPreference ReadPreference { get; set; }
        public ReadConcern ReadConcern { get; set; }
        public Encoding ReadEncoding { get; set; }
        public WriteConcern WriteConcern { get; set; }
        public Encoding WriteEncoding { get; set; }
    }
}
