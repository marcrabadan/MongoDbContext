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
                    options.Configure(x =>
                    {
                        x.Server = new MongoServerAddress("localhost");
                        x.ConnectionMode = ConnectionMode.Direct;
                        x.ReadConcern = ReadConcern.Default;
                        x.WriteConcern = WriteConcern.Acknowledged;
                    });
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
