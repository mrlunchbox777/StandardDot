using StandardDot.Abstract.Configuration;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	public class TestRedisConfigurationMetadataNoCompress : ConfigurationMetadataBase<TestRedisConfigurationNoCompress, TestRedisConfigurationMetadataNoCompress>
	{
		public override string ConfigurationLocation => "./testRedisConfigurationNoCompress.json";

		public override bool UseStream => false;

		public override string ConfigurationName => "TestRedisConfiguration";
	}
}