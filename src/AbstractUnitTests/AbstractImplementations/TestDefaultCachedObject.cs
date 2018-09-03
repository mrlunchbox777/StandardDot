using System;
using StandardDot.Abstract.Caching;

namespace StandardDot.TestClasses.AbstractImplementations
{
    internal class TestDefaultCachedObject<T> : ICachedObject<T>
    {
        public T Value { get; set; }
        public DateTime CachedTime { get; set; }
        public DateTime ExpireTime { get; set; }
    }
}