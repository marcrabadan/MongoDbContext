using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MongoDbContext.Documents;
using MongoDbContext.IntegrationTests.Contexts;

namespace MongoDbContext.IntegrationTests.Fixtures
{
    public class SocialContextFixture<TContext> : IDisposable where TContext : MongoDbContext
    {
        public SocialContextFixture()
        {
            var services = new ServiceCollection();
            services.AddMongoDbContext<TContext>(options =>
            {
                options.ConnectionString("mongodb://localhost:27017");
            }, ServiceLifetime.Transient, ServiceLifetime.Singleton);

            var serviceProvider = services.BuildServiceProvider();
            Context = serviceProvider.GetService<TContext>();
        }

        public TContext Context { get; protected set; }
        
        public void Dispose()
        {
            if (Context != null)
                Context = null;
        }
    }
}
