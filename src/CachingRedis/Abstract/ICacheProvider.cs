using StackExchange.Redis;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheProvider
	{
		string GetValueToCache<T>(RedisCachedObject<T> cachedObject);

		RedisCachedObject<T> GetCachedValue<T>(string redisValue, IRedisService service);

		RedisCachedObject<T> CreateCachedValue<T>(RedisId redisKey = null);

		ConnectionMultiplexer GetRedis();

		IDatabase GetDatabase();

		string CompressValue(string redisValue);

		string DecompressValue(string redisValue);
	}
}