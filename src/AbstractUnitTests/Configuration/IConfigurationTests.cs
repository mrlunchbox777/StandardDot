using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace StandardDot.Abstract.UnitTests.Configuration
{
    public class IConfigurationTests
    {
        [Fact]
        public void Properties()
        {
            IConfiguration<TestConfiguration, TestConfigurationMetadata> configuration = new TestConfiguration();
            Assert.Null(configuration.ConfigurationMetadata);
            configuration.ConfigurationMetadata = new TestConfigurationMetadata();
            Assert.NotNull(configuration.ConfigurationMetadata);
        }
    }
}