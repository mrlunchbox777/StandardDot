using System;
using Moq;
using StandardDot.Abstract.Caching;
using Xunit;

namespace StandardDot.Abstract.UnitTests.Caching
{
    public class ICachedObjectTests
    {
        [Fact]
        public void Properties()
        {
            Mock<ICachedObject<string>> cachedObjectProxy = new Mock<ICachedObject<string>>(MockBehavior.Strict);
            cachedObjectProxy.SetupAllProperties();
            ICachedObject<string> cachedObject = cachedObjectProxy.Object;
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