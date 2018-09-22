using System;
using System.Collections.Generic;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface IRedisService
	{
		RedisServiceType ServiceType { get; }

		IEnumerable<RedisId> GetKeys<T>(IEnumerable<RedisId> keys);

		IEnumerable<RedisId> GetKey<T>(RedisId key);

		IEnumerable<RedisCachedObject<T>> GetValues<T>(IEnumerable<RedisId> keys);

		IEnumerable<RedisCachedObject<T>> GetValue<T>(RedisId key);

		IEnumerable<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> values);

		IEnumerable<RedisCachedObject<T>> SetValue<T>(RedisCachedObject<T> value);

		void DeleteValues(IEnumerable<RedisId> keys);

		void DeleteValue(RedisId key);

		long KeyCount();

		/// <summary>
		/// Gets the TTL for a key
		/// </summary>
		/// <param name="key">The key to get a TTL for</param>
		/// <param name="dataContractResolver">The datacontract resolver to use for serialization (polymorphic dtos)</param>
		/// <returns>Time to live from redis</returns>
		Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(RedisId key);

		/// <summary>
		/// Gets the TTL for a key
		/// </summary>
		/// <param name="keys">The keys to get TTLs for</param>
		/// <param name="dataContractResolver">The datacontract resolver to use for serialization (polymorphic dtos)</param>
		/// <returns>Times to live from redis</returns>
		Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> keys);
	}
}

