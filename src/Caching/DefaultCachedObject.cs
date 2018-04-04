using System;
using StandardDot.Abstract.Caching;

namespace StandardDot.Caching
{
    public class DefaultCachedObject<T> : ICachedObject<T>
    {
        public DateTime CachedTime { get; set; }

        public DateTime ExpireTime { get; set; }

        public T Value { get; set; }
        public bool RetrievedSuccesfully { get; set; }
    }
}