using System;

namespace MongoDbFramework.Extensions
{
    public static class DocumentExtensions
    {
        public static void Created(this Document document)
        {
            document.CreatedAt = DateTime.Now;
            document.ModifiedAt = DateTime.Now;
        }

        public static void Modified(this Document document)
        {
            document.ModifiedAt = DateTime.Now;
        }
    }
}
