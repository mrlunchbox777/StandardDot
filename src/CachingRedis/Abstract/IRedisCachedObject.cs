using System.Runtime.Serialization;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface IRedisCachedObject
	{
		RedisId Id { get; set; }

		CacheInfo Metadata { get; set; }

		CacheEntryStatus? Status { get; set; }
	}
}