using System;
using Moq;
using Moq.Protected;
using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.AbstractImplementations;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace StandardDot.Abstract.UnitTests.Configuration
{
	public class ConfigurationServiceBaseTests
	{
		[Fact]
		public void Properties()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);

			Assert.Equal(cacheProxy.Object, serviceProxy.Object.Cache);
			serviceProxy.VerifyGet(x => x.Cache);
		}

		[Fact]
		public void ResetCachedConfigurationsTest()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);
			Assert.Throws<Moq.MockException>(() => serviceProxy.Object.ResetCachedConfigurations());

			cacheProxy.Setup(x => x.ResetCache());
			serviceProxy = GenerateService(cacheProxy);
			serviceProxy.Object.ResetCachedConfigurations();
		}

		[Fact]
		public void ClearConfigurationWithValueTest()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);
			Assert.Throws<Moq.MockException>(() => serviceProxy.Object.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>(null));

			cacheProxy = GenerateCache(false);
			serviceProxy = GenerateService(cacheProxy);
			serviceProxy.Object.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>(null);
		}

		[Fact]
		public void ClearConfigurationWithoutValueTest()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);
			Assert.Throws<Moq.MockException>(() => serviceProxy.Object.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>());

			cacheProxy = GenerateCache(false);
			serviceProxy = GenerateService(cacheProxy);
			serviceProxy.Object.ClearConfiguration<TestConfiguration, TestConfigurationMetadata>();
		}

		[Fact]
		public void GetConfigurationWithValueTest()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);
			Assert.Throws<Moq.MockException>(() => serviceProxy.Object.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(null));

			cacheProxy = GenerateCache(false);
			serviceProxy = GenerateService(cacheProxy);
			Assert.Null(serviceProxy.Object.GetConfiguration<TestConfiguration, TestConfigurationMetadata>(null));
		}

		[Fact]
		public void GetConfigurationWithoutValueTest()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);
			Assert.Throws<Moq.MockException>(() => serviceProxy.Object.GetConfiguration<TestConfiguration, TestConfigurationMetadata>());

			cacheProxy = GenerateCache(false);
			serviceProxy = GenerateService(cacheProxy);
			Assert.Null(serviceProxy.Object.GetConfiguration<TestConfiguration, TestConfigurationMetadata>());
		}

		[Fact]
		public void DoesConfigurationExistWithValueTest()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);
			Assert.Throws<Moq.MockException>(() => serviceProxy.Object.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(null));

			cacheProxy = GenerateCache(false);
			serviceProxy = GenerateService(cacheProxy);
			Assert.False(serviceProxy.Object.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>(null));
		}

		[Fact]
		public void DoesConfigurationExistWithoutValueTest()
		{
			Mock<ConfigurationCacheBase> cacheProxy = GenerateCache();
			Mock<ConfigurationServiceBase> serviceProxy = GenerateService(cacheProxy);
			Assert.Throws<Moq.MockException>(() => serviceProxy.Object.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>());

			cacheProxy = GenerateCache(false);
			serviceProxy = GenerateService(cacheProxy);
			Assert.False(serviceProxy.Object.DoesConfigurationExist<TestConfiguration, TestConfigurationMetadata>());
		}

		private static Mock<ConfigurationServiceBase> GenerateService(Mock<ConfigurationCacheBase> cacheProxy)
		{
			Mock<ConfigurationServiceBase> configurationServiceProxy
				= new Mock<ConfigurationServiceBase>(MockBehavior.Loose, cacheProxy.Object);
			configurationServiceProxy.CallBase = true;
			return configurationServiceProxy;
		}

		private static Mock<ConfigurationCacheBase> GenerateCache(bool useStrict = true)
		{
			TestSerializationService jsonSerializer = new TestSerializationService();
			TestMemoryCachingService cachingService = new TestMemoryCachingService(TimeSpan.FromDays(1), false);
			Mock<ConfigurationCacheBase> cacheProxy =
				new Mock<ConfigurationCacheBase>(useStrict ? MockBehavior.Strict : MockBehavior.Loose,
					cachingService, jsonSerializer, TimeSpan.FromDays(1));

			return cacheProxy;
		}
	}
}