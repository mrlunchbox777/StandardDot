using Moq;
using StandardDot.Abstract.Configuration;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests.Configuration
{
    public class IConfigurationCacheTests
    {
        [Fact]
        public void Properties()
        {
            Mock<IConfigurationCache> cacheProxy = new Mock<IConfigurationCache>(MockBehavior.Strict);
            cacheProxy.SetupGet(x => x.NumberOfConfigurations).Returns(4);
            Assert.Equal(4, cacheProxy.Object.NumberOfConfigurations);
        }

        [Fact]
        public void ResetCacheTest()
        {
            Mock<IConfigurationCache> cacheProxy = new Mock<IConfigurationCache>(MockBehavior.Strict);
            cacheProxy.Setup(x => x.ResetCache());
            cacheProxy.Object.ResetCache();
            cacheProxy.Verify(x => x.ResetCache());
        }
    }
}