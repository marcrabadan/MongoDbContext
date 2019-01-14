using Microsoft.Extensions.DependencyInjection;
using System;
using MongoDbFramework.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Collections.Generic;

namespace MongoDbFramework.IntegrationTests.Fixtures
{
    public class SocialContextFixture<TContext> where TContext : MongoDbContext
    {
        public SocialContextFixture()
        {
            var services = new ServiceCollection();
            services.AddMongoDbContext<TContext>(options =>
            {
                options.Configure(c =>
                {
                    options.ConnectionString("mongodb://localhost:27017,localhost:27018,localhost:27019/?replicaSet=rs1&readPreference=primary");
                });
            }, ServiceLifetime.Transient, ServiceLifetime.Singleton);

            this.Container = services.BuildServiceProvider();
            this.Context = this.Container.GetService<TContext>();
        }

        public IServiceProvider Container { get; protected set; }
        public TContext Context { get; protected set; }

        public void Dispose()
        {
            this.Container = null;
            this.Context = null;
        }
    }
}
