using StackExchange.Redis;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheProvider
	{
		RedisValue GetValueToCache<T>(RedisCachedObject<T> cachedObject);

		RedisCachedObject<T> GetCachedValue<T>(RedisValue redisValue, IRedisService service);

		RedisCachedObject<T> CreateCachedValue<T>(RedisId redisKey = null);

		RedisId GetRedisId(RedisId key);

		RedisId GetRedisId(string key, bool tryJson = true);

		ConnectionMultiplexer GetRedis();

		IDatabase GetDatabase();

		byte[] CompressValue(RedisValue redisValue);

		RedisValue DecompressValue(byte[] redisValue);

		RedisCachedObject<T> ChangeType<T, TK>(RedisCachedObject<TK> source);
	}
}