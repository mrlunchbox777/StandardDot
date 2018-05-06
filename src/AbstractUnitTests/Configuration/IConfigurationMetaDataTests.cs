using System;
using Moq;
using StandardDot.Abstract.Caching;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace Abstract.UnitTests.Configuration
{
    public class IConfigurationMetaDataTests
    {
        [Fact]
        public void PropertiesFile()
        {
            TestConfigurationMetadata metaData = new TestConfigurationMetadata();
            Assert.False(metaData.UseStream);
            Assert.Equal("./testConfigurationJson.json", metaData.ConfigurationLocation);
            Assert.Equal(typeof(TestConfiguration), metaData.ConfigurationType);
            Assert.Equal("TestConfiguration", metaData.ConfigurationName);
            Assert.Null(metaData.GetConfigurationStream); 
        }
        
        [Fact]
        public void PropertiesStream()
        {
            TestConfigurationMetadataStream metaData = new TestConfigurationMetadataStream();
            Assert.True(metaData.UseStream);
            Assert.Null(metaData.ConfigurationLocation);
            Assert.Equal(typeof(TestConfigurationStream), metaData.ConfigurationType);
            Assert.Equal("TestConfigurationStream", metaData.ConfigurationName);
            Assert.NotNull(metaData.GetConfigurationStream); 
        }
    }
}