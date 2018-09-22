using StackExchange.Redis;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheProvider
	{
		string GetValueToCache<T>(RedisCachedObject<T> cachedObject);

		RedisCachedObject<T> GetCachedValue<T>(string redisValue);

		ConnectionMultiplexer GetRedis();

		IDatabase GetDatabase();
	}
}