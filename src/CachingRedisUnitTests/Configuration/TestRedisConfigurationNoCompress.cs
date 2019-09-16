using System.Runtime.Serialization;
using StandardDot.Abstract.Configuration;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	[DataContract]
	public class TestRedisConfigurationNoCompress : ConfigurationBase<TestRedisConfigurationNoCompress, TestRedisConfigurationMetadataNoCompress>
	{
		[DataMember(Name = "redisSettings")]
		public RedisServiceSettings RedisSettings { get; set; }
	}
}