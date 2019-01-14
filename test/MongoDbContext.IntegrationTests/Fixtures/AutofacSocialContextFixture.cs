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
                options.ConnectionString("mongodb://localhost:27017,localhost:27018,localhost:27019/?replicaSet=rs1&readPreference=primary");                               
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
