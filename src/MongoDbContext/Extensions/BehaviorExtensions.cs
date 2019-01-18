using MongoDB.Driver;

namespace MongoDbFramework
{
    public static class BehaviorExtensions
    {
        public static TransactionOptions ToTransactionOptions(this Behavior behavior)
        {
            if (behavior == null)
                return default(TransactionOptions);

            return new TransactionOptions(behavior.ReadConcern, behavior.ReadPreference, behavior.WriteConcern);
        }

        public static MongoDatabaseSettings ToMongoDatabaseSettings(this Behavior behavior)
        {
            if (behavior == null)
                return default(MongoDatabaseSettings);

            return new MongoDatabaseSettings
            {
                ReadPreference = behavior.ReadPreference,
                ReadConcern = behavior.ReadConcern,                
                WriteConcern = behavior.WriteConcern
            };
        }
    }
}
