using Moq;
using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace StandardDot.Abstract.UnitTests.Configuration
{
	public class IConfigurationServiceTests
	{
		[Fact]
		public void Properties()
		{
			Mock<IConfigurationCache> cacheProxy = new Mock<IConfigurationCache>(MockBehavior.Strict);
			Mock<IConfigurationService> serviceProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			serviceProxy.SetupGet(x => x.Cache).Returns(cacheProxy.Object);
			Assert.Equal(cacheProxy.Object, serviceProxy.Object.Cache);
			serviceProxy.Verify(x => x.Cache);
		}

		[Fact]
		public void ResetCachedConfigurationsTest()
		{
			Mock<IConfigurationService> serviceProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			serviceProxy.Setup(x => x.ResetCachedConfigurations());
			serviceProxy.Object.ResetCachedConfigurations();
			serviceProxy.Verify(x => x.ResetCachedConfigurations());
		}

		[Fact]
		public void ClearConfigurationTest()
		{
			Mock<TestConfigurationMetadata> metadataProxy = new Mock<TestConfigurationMetadata>(MockBehavior.Strict);
			Mock<IConfigurationService> cacheProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			cacheProxy.Setup(x => x.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object));
			cacheProxy.Object.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object);
			cacheProxy.Verify(x => x.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object));
		}

		[Fact]
		public void ClearConfigurationDefaultsTest()
		{
			Mock<IConfigurationService> cacheProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			cacheProxy.Setup(x => x.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>(null));
			cacheProxy.Object.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>();
			cacheProxy.Verify(x => x.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>(null));
		}

		[Fact]
		public void GetConfigurationTest()
		{
			Mock<TestConfiguration> configurationProxy = new Mock<TestConfiguration>(MockBehavior.Strict);
			Mock<TestConfigurationMetadata> metadataProxy = new Mock<TestConfigurationMetadata>(MockBehavior.Strict);
			Mock<IConfigurationService> cacheProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			cacheProxy.Setup(x => x.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object)).Returns(configurationProxy.Object);
			Assert.Equal(configurationProxy.Object, cacheProxy.Object.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object));
			cacheProxy.Verify(x => x.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object));
		}

		[Fact]
		public void GetConfigurationDefaultsTest()
		{
			Mock<TestConfiguration> configurationProxy = new Mock<TestConfiguration>(MockBehavior.Strict);
			Mock<IConfigurationService> cacheProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			cacheProxy.Setup(x => x.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(null)).Returns(configurationProxy.Object);
			Assert.Equal(configurationProxy.Object, cacheProxy.Object.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(null));
			cacheProxy.Verify(x => x.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(null));
		}

		[Fact]
		public void DoesConfigurationExistTest()
		{
			Mock<TestConfigurationMetadata> metadataProxy = new Mock<TestConfigurationMetadata>(MockBehavior.Strict);
			Mock<IConfigurationService> cacheProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			cacheProxy.Setup(x => x.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object)).Returns(true);
			Assert.True(cacheProxy.Object.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object));
			cacheProxy.Verify(x => x.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(metadataProxy.Object));
		}

		[Fact]
		public void DoesConfigurationExistDefaultsTest()
		{
			Mock<IConfigurationService> cacheProxy = new Mock<IConfigurationService>(MockBehavior.Strict);
			cacheProxy.Setup(x => x.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(null)).Returns(true);
			Assert.True(cacheProxy.Object.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(null));
			cacheProxy.Verify(x => x.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(null));
		}
	}
}