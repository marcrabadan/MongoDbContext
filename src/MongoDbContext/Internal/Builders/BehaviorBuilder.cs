using MongoDB.Driver;
using System;
using System.Text;

namespace MongoDbFramework
{
    public class BehaviorBuilder<T> where T : IDocument
    {
        private Action<BehaviorBuilder<T>> _apply;

        public BehaviorBuilder(Action<BehaviorBuilder<T>> apply)
        {
            _apply = apply;
        }
        
        internal ReadPreference ReadPreference { get; set; }
        internal ReadConcern ReadConcern { get; set; }
        internal Encoding ReadEncoding { get; set; }
        internal WriteConcern WriteConcern { get; set; }
        internal Encoding WriteEncoding { get; set; }

        public BehaviorBuilder<T> WithReadPreference(ReadPreference readPreference)
        {
            ReadPreference = readPreference;
            _apply(this);
            return this;
        }

        public BehaviorBuilder<T> WithReadConcern(ReadConcern readConcern)
        {
            ReadConcern = readConcern;
            _apply(this);
            return this;
        }

        public BehaviorBuilder<T> WithReadEncoding(Encoding readEncoding)
        {
            ReadEncoding = readEncoding;
            _apply(this);
            return this;
        }

        public BehaviorBuilder<T> WithWriteConcern(WriteConcern writeConcern)
        {
            WriteConcern = writeConcern;
            _apply(this);
            return this;
        }

        public BehaviorBuilder<T> WithWriteEncoding(Encoding writeEncoding)
        {
            WriteEncoding = writeEncoding;
            _apply(this);
            return this;
        }

        internal Behavior Build()
        {
            return new Behavior
            {
                ReadPreference = ReadPreference,
                ReadConcern = ReadConcern,
                ReadEncoding = ReadEncoding ?? Encoding.UTF8,
                WriteConcern = WriteConcern,
                WriteEncoding = WriteEncoding ?? Encoding.UTF8
            };        
        }
    }
}
