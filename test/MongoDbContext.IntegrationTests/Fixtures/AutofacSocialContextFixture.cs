using Autofac;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDbFramework.Autofac;
using System.Collections.Generic;
using System.Net;

namespace MongoDbFramework.IntegrationTests.Fixtures
{
    public class AutofacSocialContextFixture<TContext> where TContext : MongoDbContext
    {
        public AutofacSocialContextFixture()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.AddMongoDbContext<TContext>(options =>
            {
                options.Configure(x =>
                {
                    x.Server = new MongoServerAddress("localhost");
                    x.ConnectionMode = ConnectionMode.Direct;
                    x.ReadConcern = ReadConcern.Default;
                    x.WriteConcern = WriteConcern.Acknowledged;
                });
            }, LifeTime.Transient, LifeTime.Singleton);

            this.Container = containerBuilder.Build();
            this.Context = this.Container.Resolve<TContext>();
        }

        public IContainer Container { get; protected set; }
        public TContext Context { get; protected set; }

        public void Dispose()
        {
            this.Container?.Dispose();
            this.Context = null;
        }
    }
}
