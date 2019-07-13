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
                options.Configure(x =>
                {
                    x.Server = new MongoServerAddress("localhost");
                    x.ConnectionMode = ConnectionMode.Direct;
                    x.ReadConcern = ReadConcern.Default;
                    x.WriteConcern = WriteConcern.Acknowledged;
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
