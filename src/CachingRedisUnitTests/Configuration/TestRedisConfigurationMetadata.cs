using StandardDot.Abstract.Configuration;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	public class TestRedisConfigurationMetadata : ConfigurationMetadataBase<TestRedisConfiguration, TestRedisConfigurationMetadata>
	{
		public override string ConfigurationLocation => "./testRedisConfiguration.json";

		public override bool UseStream => false;

		public override string ConfigurationName => "TestRedisConfiguration";
	}
}