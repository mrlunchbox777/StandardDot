using StandardDot.Abstract.Configuration;
using StandardDot.ConfigurationUnitTests.TestConfigurations;

namespace StandardDot.ConfigurationUnitTests.TestConfigurationMetadatas
{
    public class TestConfigurationMetadata : ConfigurationMetadataBase<TestConfiguration, TestConfigurationMetadata>
    {
        public override string ConfigurationLocation => "./testConfigurationJson.json";

        public override bool UseStream => false;

        public override string ConfigurationName => "TestConfiguration";
    }
}