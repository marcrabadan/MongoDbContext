using Autofac;
using MongoDbFramework.Autofac;

namespace MongoDbFramework.IntegrationTests.Fixtures
{
    public class AutofacSocialContextFixture<TContext> where TContext : MongoDbContext
    {
        public AutofacSocialContextFixture()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.AddMongoDbContext<TContext>(options =>
            {
                options.ConnectionString("mongodb://localhost:27017");
            }, LifeTime.Transient, LifeTime.Singleton);

            var container = containerBuilder.Build();
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
