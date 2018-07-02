using System;

namespace MongoDbContext.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SuppressCollectionInitializationAttribute : Attribute
    {
    }
}
