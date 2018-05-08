using System;
using System.IO;
using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace Abstract.UnitTests.Configuration
{
    public class ConfigurationMetaDataBaseTests
    {
        [Fact]
        public void PropertiesFile()
        {
            ConfigurationMetadataBase<TestConfiguration, TestConfigurationMetadata> metaData = new TestConfigurationMetadata();
            Assert.False(metaData.UseStream);
            Assert.Equal("./testConfigurationJson.json", metaData.ConfigurationLocation);
            Assert.Equal(typeof(TestConfiguration), metaData.ConfigurationType);
            Assert.Equal("TestConfiguration", metaData.ConfigurationName);
            Assert.Null(metaData.GetConfigurationStream); 
        }
        
        [Fact]
        public void PropertiesStream()
        {
            ConfigurationMetadataBase<TestConfigurationStream, TestConfigurationMetadataStream> metaData = new TestConfigurationMetadataStream();
            Assert.True(metaData.UseStream);
            Assert.Null(metaData.ConfigurationLocation);
            Assert.Equal(typeof(TestConfigurationStream), metaData.ConfigurationType);
            Assert.Equal("TestConfigurationStream", metaData.ConfigurationName);
            Assert.NotNull(metaData.GetConfigurationStream); 
        }

        [Fact]
        public void ConstructorFileTests()
        {
            const string location = "./testConfigurationJson.json";
            ConfigurationMetadataBase<TestConfigurationConstructor, TestConfigurationConstructorMetadata> metaData
                = new TestConfigurationConstructorMetadata(location);
            Assert.False(metaData.UseStream);
            Assert.Equal(location, metaData.ConfigurationLocation);
            Assert.Equal(typeof(TestConfigurationConstructor), metaData.ConfigurationType);
            Assert.Equal("TestConfigurationConstructor", metaData.ConfigurationName);
            Assert.Null(metaData.GetConfigurationStream); 
        }

        [Fact]
        public void ConstructorStreamTests()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                Func<Stream> getStream = () => stream;
                ConfigurationMetadataBase<TestConfigurationConstructor, TestConfigurationConstructorMetadata> metaData
                    = new TestConfigurationConstructorMetadata(getStream);
                Assert.True(metaData.UseStream);
                Assert.Equal(stream, metaData.GetConfigurationStream());
                Assert.Equal(typeof(TestConfigurationConstructor), metaData.ConfigurationType);
                Assert.Equal("TestConfigurationConstructor", metaData.ConfigurationName);
                Assert.Null(metaData.ConfigurationLocation); 
            }
        }
    }
}