using System;
using StandardDot.Abstract.Caching;

namespace StandardDot.Caching
{
    /// <summary>
    /// A Basic Caching Object Wrapper
    /// </summary>
    /// <typeparam name="T">The wrapped object Type</typeparam>
    public class DefaultCachedObject<T> : ICachedObject<T>
    {
        public DateTime CachedTime { get; set; }

        public DateTime ExpireTime { get; set; }

        public T Value { get; set; }

        public bool RetrievedSuccesfully { get; set; }
    }
}