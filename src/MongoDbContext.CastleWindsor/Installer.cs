using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;

namespace MongoDbFramework.CastleWindsor
{
    public static class Installer
    {
        public static void AddMongoDbContext<TContext>(
               this IWindsorContainer containerBuilder,
               Action<MongoDbOptionBuilder> options,
               LifestyleType contextLifestyle = LifestyleType.Scoped,
               LifestyleType optionsLifestyle = LifestyleType.Scoped)
               where TContext : MongoDbContext
        {
            containerBuilder.AddMongoDbContext<TContext>((p, b) => options.Invoke(b), contextLifestyle, optionsLifestyle);
        }

        private static IWindsorContainer AddMongoDbContext<TContext>(
            this IWindsorContainer containerBuilder,
            Action<IKernel, MongoDbOptionBuilder> options,
            LifestyleType contextLifestyle = LifestyleType.Scoped,
            LifestyleType optionsLifestyle = LifestyleType.Scoped)
            where TContext : MongoDbContext
        {
            if (containerBuilder == null)
                throw new InvalidOperationException($"{nameof(containerBuilder)} is null.");

            if (contextLifestyle == LifestyleType.Singleton)
            {
                optionsLifestyle = LifestyleType.Singleton;
            }

            containerBuilder.Register(
                Component.For<MongoDbOptions<TContext>>()
                    .UsingFactoryMethod(kernel => MongoDbOptionsFactory<TContext>(kernel, options))
                    .LifeStyle.Is(optionsLifestyle),
                Component.For<TContext>().ImplementedBy<TContext>().LifeStyle.Is(contextLifestyle)            
            );
            return containerBuilder;
        }

        private static MongoDbOptions<TContext> MongoDbOptionsFactory<TContext>(
            IKernel kernel,
            Action<IKernel, MongoDbOptionBuilder> optionsAction)
            where TContext : MongoDbContext
        {
            var builder = new MongoDbOptionBuilder<TContext>();

            optionsAction.Invoke(kernel, builder);

            return (MongoDbOptions<TContext>)builder.Options;
        }
    }
}
