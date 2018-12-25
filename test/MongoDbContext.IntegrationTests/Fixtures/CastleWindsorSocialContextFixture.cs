using Castle.Core;
using Castle.Windsor;
using MongoDbFramework.CastleWindsor;

namespace MongoDbFramework.IntegrationTests.Fixtures
{
    public class CastleWindsorSocialContextFixture<TContext> where TContext : MongoDbContext
    {
        public CastleWindsorSocialContextFixture()
        {
            var container = new WindsorContainer();
            container.AddMongoDbContext<TContext>(options =>
            {
                options.ConnectionString("mongodb://localhost:27017");
            }, LifestyleType.Transient, LifestyleType.Singleton);

            Context = container.Resolve<TContext>();
        }

        public TContext Context { get; protected set; }

        public void Dispose()
        {
            if (Context != null)
                Context = null;
        }
    }
}
