using MongoDbFramework.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace MongoDbFramework
{
    public static class DocumentExtensions
    {
        public static PropertyInfo FindKey(this IDocument document)
        {
            return document.GetType().FindKey();
        }

        public static void SetId<TValue>(this IDocument document, TValue value)
        {
            var key = document.GetType().GetKeyPropertyInfo();
            key.SetValue(document, value);
        }

        public static PropertyInfo FindKey(this Type type)
        {
            var property = type.GetKeyPropertyInfo();
            if (property == null) throw new ArgumentException($"You have to specify a key in {type} document with {nameof(Abstractions.KeyAttribute)}.");
            return property;
        }

        private static PropertyInfo GetKeyPropertyInfo(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(c => c.GetCustomAttributes(typeof(Abstractions.KeyAttribute), false).Any()).FirstOrDefault();
        }
    }
}
