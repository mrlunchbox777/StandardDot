using System.Runtime.Serialization;
using StandardDot.Abstract.Configuration;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.UnitTests.Configuration
{
	[DataContract]
	public class TestRedisConfiguration : ConfigurationBase<TestRedisConfiguration, TestRedisConfigurationMetadata>
	{
		[DataMember(Name = "redisSettings")]
		public RedisServiceSettings RedisSettings { get; set; }
	}
}