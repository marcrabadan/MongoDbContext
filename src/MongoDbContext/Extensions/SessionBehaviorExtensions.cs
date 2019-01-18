using MongoDB.Driver;

namespace MongoDbFramework
{
    public static class SessionBehaviorExtensions
    {
        public static ClientSessionOptions ToClientSessionOptions(this SessionBehavior sessionBehavior)
        {
            if (sessionBehavior == null)
                return default(ClientSessionOptions);

            return new ClientSessionOptions
            {
                CausalConsistency = sessionBehavior.CasualConsistency,
                DefaultTransactionOptions = new TransactionOptions(sessionBehavior.ReadConcern, sessionBehavior.ReadPreference, sessionBehavior.WriteConcern)
            };
        }
    }
}
