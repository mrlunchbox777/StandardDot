using System;

namespace StandardDot.Abstract.Caching
{
    public interface ICachedObject<T>
    {
        DateTime CachedTime { get; set; }

        DateTime ExpireTime { get; set; }

        T Value { get; set; }
    }
}