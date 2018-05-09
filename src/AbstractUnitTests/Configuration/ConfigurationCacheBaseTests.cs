using Moq;
using StandardDot.Abstract.Configuration;
using Xunit;

namespace Abstract.UnitTests.Configuration
{
    public class ConfigurationCacheBaseTests
    {
        [Fact]
        public void Properties()
        {
            Mock<ConfigurationCacheBase> cacheProxy = new Mock<ConfigurationCacheBase>(MockBehavior.Strict);
            cacheProxy.SetupGet(x => x.NumberOfConfigurations).Returns(4);
            Assert.Equal(4, cacheProxy.Object.NumberOfConfigurations);
        }

        [Fact]
        public void ResetCacheTest()
        {
            Mock<ConfigurationCacheBase> cacheProxy = new Mock<ConfigurationCacheBase>(MockBehavior.Strict);
            cacheProxy.Setup(x => x.ResetCache());
            cacheProxy.Object.ResetCache();
            cacheProxy.Verify(x => x.ResetCache());
        }
    }
}