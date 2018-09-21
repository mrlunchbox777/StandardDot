using System.Collections.Generic;
using StandardDot.Caching.Redis.Dto;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheProvider
	{
		List<RedisCachedObject<T>> GetValuesByKeys<T>(string[] keys);

		List<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> valuesToCache);

		void DeleteValuesByKeys(string[] keys);
	}
}