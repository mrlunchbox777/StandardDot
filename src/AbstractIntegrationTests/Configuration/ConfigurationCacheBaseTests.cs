using System;
using Moq;
using StandardDot.Abstract.Configuration;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests.Configuration
{
    public class ConfigurationCacheBaseTests
    {
        [Fact]
        public void Properties()
        {
            Mock<ConfigurationCacheBase> cacheProxy = new Mock<ConfigurationCacheBase>(MockBehavior.Strict, null, null, TimeSpan.FromDays(1));
            cacheProxy.SetupGet(x => x.NumberOfConfigurations).Returns(4);
            Assert.Equal(4, cacheProxy.Object.NumberOfConfigurations);
        }

        [Fact]
        public void ResetCacheTest()
        {
            Mock<ConfigurationCacheBase> cacheProxy = new Mock<ConfigurationCacheBase>(MockBehavior.Strict, null, null, TimeSpan.FromDays(1));
            cacheProxy.Setup(x => x.ResetCache());
            cacheProxy.Object.ResetCache();
            cacheProxy.Verify(x => x.ResetCache());
        }
    }
}