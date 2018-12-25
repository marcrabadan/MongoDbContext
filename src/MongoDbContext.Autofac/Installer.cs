using Autofac;
using Autofac.Core;
using System;

namespace MongoDbFramework.Autofac
{
    public static class Installer
    {
        public static void AddMongoDbContext<TContext>(
               this ContainerBuilder containerBuilder,
               Action<MongoDbOptionBuilder> options,
               LifeTime contextLifetime = LifeTime.Scoped,
               LifeTime optionsLifetime = LifeTime.Scoped)
               where TContext : MongoDbContext
        {
            containerBuilder.AddMongoDbContext<TContext>((p, b) => options.Invoke(b), contextLifetime, optionsLifetime);
        }

        private static ContainerBuilder AddMongoDbContext<TContext>(
            this ContainerBuilder containerBuilder,
            Action<IComponentContext, MongoDbOptionBuilder> options,
            LifeTime contextLifetime = LifeTime.Scoped,
            LifeTime optionsLifetime = LifeTime.Scoped)
            where TContext : MongoDbContext
        {
            if (containerBuilder == null)
                throw new InvalidOperationException($"{nameof(containerBuilder)} is null.");

            if (contextLifetime == LifeTime.Singleton)
            {
                optionsLifetime = LifeTime.Singleton;
            }

            var optionRegistration = containerBuilder
                .Register<MongoDbOptions<TContext>>(c => MongoDbOptionsFactory<TContext>(c, options))
                .OnlyIf(reg => !reg.IsRegistered(new TypedService(typeof(MongoDbOptions<TContext>))));

            switch (optionsLifetime)
            {
                case LifeTime.Singleton:
                    optionRegistration.SingleInstance();
                    break;
                case LifeTime.Scoped | LifeTime.Thread:
                    optionRegistration.InstancePerLifetimeScope();
                    break;
                case LifeTime.Transient:
                    optionRegistration.InstancePerDependency();
                    break;
                case LifeTime.Request:
                    optionRegistration.InstancePerRequest();
                    break;
            }

            var contextRegistration = containerBuilder
                .RegisterType<TContext>()
                .AsSelf()
                .OnlyIf(reg => !reg.IsRegistered(new TypedService(typeof(TContext))));
            
            switch (contextLifetime)
            {
                case LifeTime.Singleton:
                    contextRegistration.SingleInstance();
                    break;
                case LifeTime.Scoped | LifeTime.Thread:
                    contextRegistration.InstancePerLifetimeScope();
                    break;
                case LifeTime.Transient:
                    contextRegistration.InstancePerDependency();
                    break;
                case LifeTime.Request:
                    contextRegistration.InstancePerRequest();
                    break;
            }

            return containerBuilder;
        }

        private static MongoDbOptions<TContext> MongoDbOptionsFactory<TContext>(
            IComponentContext componentContext,
            Action<IComponentContext, MongoDbOptionBuilder> optionsAction)
            where TContext : MongoDbContext
        {
            var builder = new MongoDbOptionBuilder<TContext>();

            optionsAction.Invoke(componentContext, builder);

            return (MongoDbOptions<TContext>)builder.Options;
        }
    }
}
