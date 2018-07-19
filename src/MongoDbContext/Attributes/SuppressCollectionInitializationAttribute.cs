using System;

namespace MongoDbFramework
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SuppressCollectionInitializationAttribute : Attribute
    {
    }
}
