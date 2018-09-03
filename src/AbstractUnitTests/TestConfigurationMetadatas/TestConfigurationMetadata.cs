using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurations;

namespace StandardDot.TestClasses.TestConfigurationMetadatas
{
    public class TestConfigurationMetadata : ConfigurationMetadataBase<TestConfiguration, TestConfigurationMetadata>
    {
        public override string ConfigurationLocation => "./testConfigurationJson.json";

        public override bool UseStream => false;

        public override string ConfigurationName => "TestConfiguration";
    }
}