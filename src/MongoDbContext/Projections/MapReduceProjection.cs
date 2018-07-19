using Newtonsoft.Json;

namespace MongoDbFramework
{
    public class MapReduceProjection<T> where T : class
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        public T Value { get; set; }
    }
}
