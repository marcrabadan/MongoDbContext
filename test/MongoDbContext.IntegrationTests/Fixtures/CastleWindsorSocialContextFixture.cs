using Castle.Core;
using Castle.Windsor;
using MongoDB.Driver;
using MongoDbFramework.CastleWindsor;
using System.Collections.Generic;

namespace MongoDbFramework.IntegrationTests.Fixtures
{
    public class CastleWindsorSocialContextFixture<TContext> where TContext : MongoDbContext
    {
        public CastleWindsorSocialContextFixture()
        {
            var container = new WindsorContainer();
            container.AddMongoDbContext<TContext>(options =>
            {
                options.Configure(c =>
                {
                    options.ConnectionString("mongodb://localhost:27017,localhost:27018,localhost:27019/?replicaSet=rs1&readPreference=primary");
                });
            }, LifestyleType.Transient, LifestyleType.Singleton);

            this.Container = container;
            this.Context = this.Container.Resolve<TContext>();
        }

        public IWindsorContainer Container { get; protected set; }
        public TContext Context { get; protected set; }

        public void Dispose()
        {
            this.Container?.Dispose();
            this.Context = null;
        }
    }
}
