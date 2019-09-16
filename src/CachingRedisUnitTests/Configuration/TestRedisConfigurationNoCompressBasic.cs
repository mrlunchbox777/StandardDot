using System.Runtime.Serialization;
using StandardDot.Abstract.Configuration;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	[DataContract]
	public class TestRedisConfigurationNoCompressBasic : ConfigurationBase<TestRedisConfigurationNoCompressBasic, TestRedisConfigurationMetadataNoCompressBasic>
	{
		[DataMember(Name = "redisSettings")]
		public RedisServiceSettings RedisSettings { get; set; }
	}
}