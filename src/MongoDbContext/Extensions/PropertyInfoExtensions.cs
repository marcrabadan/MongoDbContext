using MongoDB.Bson;
using MongoDbFramework.Abstractions;
using System.Reflection;

namespace MongoDbFramework.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static BsonDocument GetBsonValue<TValue>(this PropertyInfo property, TValue value)
        {
            return new BsonDocument(new BsonElement(property.GetKeyPropertyName(), BsonValue.Create(value)));
        }
         
        public static BsonDocument GetValueFromProperty<TDocument>(this PropertyInfo property, TDocument value)
        {
            return new BsonDocument(new BsonElement(property.GetKeyPropertyName(), BsonValue.Create(property.GetValue(value, null))));
        }

        private static string GetKeyPropertyName(this PropertyInfo property)
        {
            var key = property.GetCustomAttribute<KeyAttribute>();
            return key.Name;
        }
    }
}
