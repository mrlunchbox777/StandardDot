using System.Runtime.Serialization;

namespace StandardDot.Caching.Redis.Enums
{
	[DataContract]
	public enum RedisServiceType
	{
		[EnumMember(Value = "HashSet")]
		HashSet = 0,
		
		[EnumMember(Value = "KeyValue")]
		KeyValue = 1,
		
		[EnumMember(Value = "Provider")]
		Provider = 2
	}
}
