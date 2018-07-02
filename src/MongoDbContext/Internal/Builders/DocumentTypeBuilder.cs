using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDbContext.Documents;

namespace MongoDbContext.Internal.Builders
{
    public class DocumentTypeBuilder<T> where T : Document
    {
        private Action<DocumentTypeBuilder<T>> _apply;

        public DocumentTypeBuilder(Action<DocumentTypeBuilder<T>> apply)
        {
            _apply = apply;
        }

        internal string DatabaseName { get; set; }
        internal string CollectionName { get; set; }

        public DocumentTypeBuilder<T> WithDatabase(string name)
        {
            DatabaseName = name;
            _apply(this);
            return this;
        }

        public DocumentTypeBuilder<T> WithCollection(string name)
        {
            CollectionName = name;
            _apply(this);
            return this;
        }
    }
}
