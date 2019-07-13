using System;
using System.Text;
using MongoDB.Driver;
using MongoDbFramework.Abstractions;

namespace MongoDbFramework
{
    public sealed class SessionBehaviorBuilder<T> where T : IDocument
    {
        private Action<SessionBehaviorBuilder<T>> _apply;

        public SessionBehaviorBuilder(Action<SessionBehaviorBuilder<T>> apply)
        {
            _apply = apply;
        }
        
        internal bool CasualConsistency { get; set; }
        internal ReadPreference ReadPreference { get; set; }
        internal ReadConcern ReadConcern { get; set; }
        internal Encoding ReadEncoding { get; set; }
        internal WriteConcern WriteConcern { get; set; }
        internal Encoding WriteEncoding { get; set; }

        public SessionBehaviorBuilder<T> WithReadPreference(ReadPreference readPreference)
        {
            ReadPreference = readPreference;
            _apply(this);
            return this;
        }

        public SessionBehaviorBuilder<T> WithReadConcern(ReadConcern readConcern)
        {
            ReadConcern = readConcern;
            _apply(this);
            return this;
        }

        public SessionBehaviorBuilder<T> WithReadEncoding(Encoding readEncoding)
        {
            ReadEncoding = readEncoding;
            _apply(this);
            return this;
        }

        public SessionBehaviorBuilder<T> WithWriteConcern(WriteConcern writeConcern)
        {
            WriteConcern = writeConcern;
            _apply(this);
            return this;
        }

        public SessionBehaviorBuilder<T> WithWriteEncoding(Encoding writeEncoding)
        {
            WriteEncoding = writeEncoding;
            _apply(this);
            return this;
        }

        public SessionBehaviorBuilder<T> EnableCasualConsitency()
        {
            CasualConsistency = true;
            _apply(this);
            return this;
        }

        internal SessionBehavior Build()
        {
            return new SessionBehavior
            {
                ReadPreference = ReadPreference,
                ReadConcern = ReadConcern,
                ReadEncoding = ReadEncoding ?? Encoding.UTF8,
                WriteConcern = WriteConcern,
                WriteEncoding = WriteEncoding ?? Encoding.UTF8,
                CasualConsistency = CasualConsistency
            };
        }
    }
}
