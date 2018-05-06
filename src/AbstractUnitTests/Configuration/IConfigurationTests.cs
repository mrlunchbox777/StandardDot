using System;
using Moq;
using StandardDot.Abstract.Caching;
using StandardDot.TestClasses.TestConfigurationMetadatas;
using StandardDot.TestClasses.TestConfigurations;
using Xunit;

namespace Abstract.UnitTests.Configuration
{
    public class IConfigurationTests
    {
        [Fact]
        public void Properties()
        {
            TestConfiguration configuration = new TestConfiguration();
            Assert.Null(configuration.ConfigurationMetadata);
            configuration.ConfigurationMetadata = new TestConfigurationMetadata();
            Assert.NotNull(configuration.ConfigurationMetadata);
        }
    }
}