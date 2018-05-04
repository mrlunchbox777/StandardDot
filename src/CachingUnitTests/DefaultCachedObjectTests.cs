using System;
using StandardDot.Caching;
using Xunit;

namespace StandardDot.Caching.UnitTests
{
    public class DefaultCachedObjectTests
    {
        [Fact]
        public void Properties()
        {
            DefaultCachedObject<string> cachedObject = new DefaultCachedObject<string>();
            const string value = "a value";
            DateTime time = DateTime.UtcNow;
            DateTime expireTime = DateTime.UtcNow.AddMinutes(5);

            cachedObject.ExpireTime = expireTime;
            cachedObject.CachedTime = time;
            cachedObject.Value = value;

            Assert.Equal(expireTime, cachedObject.ExpireTime);
            Assert.Equal(time, cachedObject.CachedTime);
            Assert.Equal(value, cachedObject.Value);
        }
    }
}