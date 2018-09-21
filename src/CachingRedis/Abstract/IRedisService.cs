using System;
using System.Collections.Generic;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface IRedisService
	{
		RedisServiceType ServiceType { get; }

		void AddToCache<T>(RedisHashSetCachedObject<T> source, IDataContractResolver dataContractResolver = null);

		void DeleteFromCache(RedisId key);

		void DeleteFromCache(IEnumerable<RedisId> keys);

		T GetFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null);

		IEnumerable<RedisId> GetKeys(RedisId key, IDataContractResolver dataContractResolver = null);

		IEnumerable<string> GetKeysStrings(RedisId key, IDataContractResolver dataContractResolver = null);

		IEnumerable<T> GetListFromCache<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null);

		IEnumerable<T> GetListFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null);

		// IEnumerable<Tuple<RedisId, string>> GetStringListFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null);

		// IEnumerable<Tuple<RedisId, string>> GetStringListFromCache<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null);

		/// <summary>
		/// Gets the TTL for a key
		/// </summary>
		/// <param name="key">The key to get a TTL for</param>
		/// <param name="dataContractResolver">The datacontract resolver to use for serialization (polymorphic dtos)</param>
		/// <returns>Time to live from redis</returns>
		TimeSpan? GetTimeToLive<T>(RedisId key, IDataContractResolver dataContractResolver = null);

		/// <summary>
		/// Gets the TTL for a key
		/// </summary>
		/// <param name="keys">The keys to get TTLs for</param>
		/// <param name="dataContractResolver">The datacontract resolver to use for serialization (polymorphic dtos)</param>
		/// <returns>Times to live from redis</returns>
		Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null);
	}
}

