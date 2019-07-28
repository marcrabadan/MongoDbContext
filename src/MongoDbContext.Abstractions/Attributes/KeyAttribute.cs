using System;

namespace MongoDbFramework.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class KeyAttribute : Attribute
    {
        public string Name { get; set; } = "_id";
    }
}
