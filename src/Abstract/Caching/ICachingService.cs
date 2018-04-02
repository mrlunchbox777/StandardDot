using System;
using System.Collections.Generic;

namespace shoellibraries.Abstract.Caching
{
    public interface ICachingService : IDictionary<string, ICachedObject<object>>
    {
        TimeSpan DefaultCacheLifespan { get; }
        
        void Cache<T>(string key, ICachedObject<T> value);

        void Cache<T>(string key, T value, DateTime cachedTime, DateTime expireTime);

        ICachedObject<T> Retrieve<T>(string key);

        bool Invalidate(string key);
    }
}