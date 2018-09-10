using System;
using System.IO;
using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace StandardDot.Abstract.IntegrationTests.Configuration
{
    public class ConfigurationMetaDataBaseTests
    {
        [Fact]
        public void PropertiesFile()
        {
            ConfigurationMetadataBase<TestConfiguration, TestConfigurationMetadata> metadata = new TestConfigurationMetadata();
            Assert.False(metadata.UseStream);
            Assert.Equal("./testConfigurationJson.json", metadata.ConfigurationLocation);
            Assert.Equal(typeof(TestConfiguration), metadata.ConfigurationType);
            Assert.Equal("TestConfiguration", metadata.ConfigurationName);
            Assert.Null(metadata.GetConfigurationStream); 
        }
        
        [Fact]
        public void PropertiesStream()
        {
            ConfigurationMetadataBase<TestConfigurationStream, TestConfigurationMetadataStream> metadata = new TestConfigurationMetadataStream();
            Assert.True(metadata.UseStream);
            Assert.Null(metadata.ConfigurationLocation);
            Assert.Equal(typeof(TestConfigurationStream), metadata.ConfigurationType);
            Assert.Equal("TestConfigurationStream", metadata.ConfigurationName);
            Assert.NotNull(metadata.GetConfigurationStream); 
        }

        [Fact]
        public void ConstructorFileTests()
        {
            const string location = "./testConfigurationJson.json";
            ConfigurationMetadataBase<TestConfigurationConstructor, TestConfigurationConstructorMetadata> metadata
                = new TestConfigurationConstructorMetadata(location);
            Assert.False(metadata.UseStream);
            Assert.Equal(location, metadata.ConfigurationLocation);
            Assert.Equal(typeof(TestConfigurationConstructor), metadata.ConfigurationType);
            Assert.Equal("TestConfigurationConstructor", metadata.ConfigurationName);
            Assert.Null(metadata.GetConfigurationStream); 
        }

        [Fact]
        public void ConstructorStreamTests()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                Func<Stream> getStream = () => stream;
                ConfigurationMetadataBase<TestConfigurationConstructor, TestConfigurationConstructorMetadata> metadata
                    = new TestConfigurationConstructorMetadata(getStream);
                Assert.True(metadata.UseStream);
                Assert.Equal(stream, metadata.GetConfigurationStream());
                Assert.Equal(typeof(TestConfigurationConstructor), metadata.ConfigurationType);
                Assert.Equal("TestConfigurationConstructor", metadata.ConfigurationName);
                Assert.Null(metadata.ConfigurationLocation); 
            }
        }
    }
}