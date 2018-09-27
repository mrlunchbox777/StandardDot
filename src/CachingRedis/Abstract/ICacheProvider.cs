﻿using StackExchange.Redis;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheProvider
	{
		RedisValue GetValueToCache<T>(RedisCachedObject<T> cachedObject);

		RedisCachedObject<T> GetCachedValue<T>(RedisValue redisValue, IRedisService service);

		RedisCachedObject<T> CreateCachedValue<T>(RedisId redisKey = null);

		ConnectionMultiplexer GetRedis();

		IDatabase GetDatabase();

		byte[] CompressValue(string redisValue);

		string DecompressValue(byte[] redisValue);

		RedisCachedObject<T> ChangeType<T, TK>(RedisCachedObject<TK> source);
	}
}