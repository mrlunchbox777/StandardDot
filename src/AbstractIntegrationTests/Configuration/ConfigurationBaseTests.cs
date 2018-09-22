using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests.Configuration
{
	public class ConfigurationBaseTests
	{
		[Fact]
		public void Properties()
		{
			ConfigurationBase<TestConfiguration, TestConfigurationMetadata> configuration = new TestConfiguration();
			Assert.Null(configuration.ConfigurationMetadata);
			configuration.ConfigurationMetadata = new TestConfigurationMetadata();
			Assert.NotNull(configuration.ConfigurationMetadata);
		}

		[Fact]
		public void ConstructorTests()
		{
			TestConfigurationConstructorMetadata metadata = new TestConfigurationConstructorMetadata();
			ConfigurationBase<TestConfigurationConstructor, TestConfigurationConstructorMetadata> configuration
				= new TestConfigurationConstructor(metadata);
			Assert.NotNull(configuration.ConfigurationMetadata);
			Assert.Equal(metadata, configuration.ConfigurationMetadata);
		}
	}
}