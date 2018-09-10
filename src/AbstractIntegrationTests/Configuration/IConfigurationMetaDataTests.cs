using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests.Configuration
{
    public class IConfigurationMetaDataTests
    {
        [Fact]
        public void PropertiesFile()
        {
            IConfigurationMetadata<TestConfiguration, TestConfigurationMetadata> metadata = new TestConfigurationMetadata();
            Assert.False(metadata.UseStream);
            Assert.Equal("./testConfigurationJson.json", metadata.ConfigurationLocation);
            Assert.Equal(typeof(TestConfiguration), metadata.ConfigurationType);
            Assert.Equal("TestConfiguration", metadata.ConfigurationName);
            Assert.Null(metadata.GetConfigurationStream); 
        }
        
        [Fact]
        public void PropertiesStream()
        {
            IConfigurationMetadata<TestConfigurationStream, TestConfigurationMetadataStream> metadata = new TestConfigurationMetadataStream();
            Assert.True(metadata.UseStream);
            Assert.Null(metadata.ConfigurationLocation);
            Assert.Equal(typeof(TestConfigurationStream), metadata.ConfigurationType);
            Assert.Equal("TestConfigurationStream", metadata.ConfigurationName);
            Assert.NotNull(metadata.GetConfigurationStream); 
        }
    }
}