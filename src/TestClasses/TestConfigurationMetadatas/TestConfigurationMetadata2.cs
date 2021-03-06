using StandardDot.Abstract.Configuration;
using StandardDot.TestClasses.TestConfigurations;

namespace StandardDot.TestClasses.TestConfigurationMetadatas
{
	public class TestConfigurationMetadata2 : ConfigurationMetadataBase<TestConfiguration2, TestConfigurationMetadata2>
	{
		public override string ConfigurationLocation => "./testConfigurationJson2.json";

		public override bool UseStream => false;

		public override string ConfigurationName => "TestConfiguration2";
	}
}