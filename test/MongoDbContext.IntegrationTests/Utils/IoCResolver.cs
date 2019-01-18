using Autofac;
using Castle.Windsor;
using MongoDbFramework.IntegrationTests.Enums;
using System;

namespace MongoDbFramework.IntegrationTests.Utils
{
    public sealed class IoCResolver
    {
        private static IoCResolver instance = null;
        private static readonly object padlock = new object();

        IoCResolver(Tuple<IServiceProvider, IWindsorContainer, IContainer> ioCProviders)
        {
            this.IoCProviders = ioCProviders;
        }
        
        private Tuple<IServiceProvider, IWindsorContainer, IContainer> IoCProviders { get; set; }

        public static IoCResolver Instance(Tuple<IServiceProvider, IWindsorContainer, IContainer> ioCProviders)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new IoCResolver(ioCProviders);
                }
                return instance;
            }
        }

        public T Resolve<T>(IoCType ioCType)
        {
            T obj = default(T);
            switch (ioCType)
            {
                case IoCType.MicrosoftExtensionsDependencyInjection:
                    return (T)instance.IoCProviders.Item1.GetService(typeof(T));
                case IoCType.Autofac:
                    return instance.IoCProviders.Item3.Resolve<T>();
                case IoCType.CastleWindsor:
                    return instance.IoCProviders.Item2.Resolve<T>();
            }
            return obj;
        }
    }
}
