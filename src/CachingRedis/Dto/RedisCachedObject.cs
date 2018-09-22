using System;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
	public class RedisCachedObject<T> : DefaultCachedObject<T>, IRedisCachedObject<T>
	{
		public RedisCachedObject(string objectIdentifier)
		{
			Id = new RedisId();
			Id.ObjectIdentifier = objectIdentifier;
			Id.ServiceType = RedisServiceType.HashSet;
		}

		public RedisId Id { get; set; }

		public CacheInfo Metadata { get; set; }

		public CacheEntryStatus? Status { get; set; }
	}
}
