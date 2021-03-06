﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace MongoDbFramework.Extensions.DependencyInjection
{
    public static class Installer
    {
        public static IServiceCollection AddMongoDbContext<TContext>(
               this IServiceCollection services,
               Action<MongoDbOptionBuilder> options,
               ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
               ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
               where TContext : MongoDbContext
        {
            services.AddMongoDbContext<TContext>((p, b) => options.Invoke(b), contextLifetime, optionsLifetime);
            return services;
        }

        private static IServiceCollection AddMongoDbContext<TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, MongoDbOptionBuilder> options,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : MongoDbContext
        {
            if (serviceCollection == null)
                throw new InvalidOperationException($"{nameof(serviceCollection)} is null.");

            if (contextLifetime == ServiceLifetime.Singleton)
            {
                optionsLifetime = ServiceLifetime.Singleton;
            }

            serviceCollection.TryAdd(
                new ServiceDescriptor(
                    typeof(MongoDbOptions<TContext>),
                    p => MongoDbOptionsFactory<TContext>(p, options),
                    optionsLifetime));

            serviceCollection.TryAdd(new ServiceDescriptor(typeof(TContext), typeof(TContext), contextLifetime));

            return serviceCollection;
        }

        private static MongoDbOptions<TContext> MongoDbOptionsFactory<TContext>(
            IServiceProvider applicationServiceProvider,
            Action<IServiceProvider, MongoDbOptionBuilder> optionsAction)
            where TContext : MongoDbContext
        {
            var builder = new MongoDbOptionBuilder<TContext>();

            optionsAction.Invoke(applicationServiceProvider, builder);

            return (MongoDbOptions<TContext>)builder.Options;
        }
    }
}
