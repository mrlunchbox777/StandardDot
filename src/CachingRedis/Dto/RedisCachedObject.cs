using System;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
	public class RedisCachedObject<T> : DefaultCachedObject<T>, IRedisCachedObject
	{
		public RedisCachedObject(RedisId id)
		{
			Id = id;
		}

		public RedisId Id { get; set; }

		public CacheInfo Metadata { get; set; }

		public CacheEntryStatus? Status { get; set; }
	}
}
