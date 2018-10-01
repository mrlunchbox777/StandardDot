using StandardDot.Abstract.Configuration;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	public class TestRedisConfigurationMetadataNoCompressBasic : ConfigurationMetadataBase<TestRedisConfigurationNoCompressBasic, TestRedisConfigurationMetadataNoCompressBasic>
	{
		public override string ConfigurationLocation => "./testRedisConfigurationNoCompressBasic.json";

		public override bool UseStream => false;

		public override string ConfigurationName => "TestRedisConfiguration";
	}
}