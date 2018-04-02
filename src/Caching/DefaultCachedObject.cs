using System;
using shoellibraries.Abstract.Caching;

namespace Caching
{
    public class DefaultCachedObject<T> : ICachedObject<T>
    {
        public DateTime CachedTime { get; set; }

        public DateTime ExpireTime { get; set; }

        public T Value { get; set; }
        public bool RetrievedSuccesfully { get; set; }
    }
}