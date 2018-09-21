using System.Collections.Generic;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheProvider
	{
		List<RedisCachedObject<T>> GetValuesByKeys<T>(IEnumerable<RedisId> keys);

		List<RedisCachedObject<T>> GetValue<T>(RedisId[] key);

		List<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> values);

		List<RedisCachedObject<T>> SetValue<T>(RedisCachedObject<T> value);

		void DeleteValues(IEnumerable<string> keys);
	}
}