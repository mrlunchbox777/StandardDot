using StandardDot.Abstract.Configuration;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	public class TestRedisConfigurationMetadataBasic : ConfigurationMetadataBase<TestRedisConfigurationBasic, TestRedisConfigurationMetadataBasic>
	{
		public override string ConfigurationLocation => "./testRedisConfigurationBasic.json";

		public override bool UseStream => false;

		public override string ConfigurationName => "TestRedisConfiguration";
	}
}