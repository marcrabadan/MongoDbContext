﻿namespace MongoDbContext.ResultModels
{
    public class ReduceResult<T>
    {
        public string Id { get; set; }
        
        public T value { get; set; }
    }
}
