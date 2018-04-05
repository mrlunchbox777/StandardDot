using System;

namespace StandardDot.Abstract.Caching
{
    /// <summary>
    /// An interface that defines how objects are cached
    /// </summary>
    /// <typeparam name="T">The type of object to be cached</typeparam>
    public interface ICachedObject<T>
    {
        DateTime CachedTime { get; set; }

        DateTime ExpireTime { get; set; }

        T Value { get; set; }
    }
}