using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Service
{
	/// <summary>
	/// Check redis service for summaries
	/// </summary>
	internal class BasicRedisService : ARedisService
	{
		public BasicRedisService(RedisService redisService)
		{
			RedisService = redisService;
		}

		public  override RedisService RedisService { get; }

		public override RedisServiceType ServiceType => RedisServiceType.KeyValue;

		protected override void ServiceAdd(RedisId key, RedisValue value, DateTime expiration)
		{
			RedisService.Database.StringSet(key.FullKey, value, expiration - DateTime.UtcNow);
		}

		protected override RedisValue ServiceGet(RedisId key)
		{
			return RedisService.Database.StringGet(key.FullKey);
		}

		public override bool ContainsKey(RedisId key)
		{
			if (key == null)
			{
				return false;
			}
			return RedisService.Database.KeyExists(key.FullKey);
		}

		public override long ContainsKeys(IEnumerable<RedisId> keys)
		{
			if (!(keys?.Any() ?? false))
			{
				return 0;
			}
			return RedisService.Database.KeyExists(keys.Select(x => (RedisKey)((string)x)).ToArray());
		}

		public override bool DeleteValue(RedisId key)
		{
			if (string.IsNullOrWhiteSpace(key?.FullKey))
			{
				return false;
			}
			IEnumerable<RedisKey> keys = GetKey<object>(key).Select(x => (RedisKey)x.FullKey);
			return RedisService.Database.KeyDelete(keys.ToArray()) > 0;
		}

		public override long DeleteValues(IEnumerable<RedisId> keys)
		{
			if (!(keys?.Any() ?? false))
			{
				return 0;
			}
			IEnumerable<RedisKey> redisKeys = GetKeys<object>(keys).Select(x => (RedisKey)x.FullKey);
			return RedisService.Database.KeyDelete(redisKeys.ToArray());
		}

		public override IEnumerable<RedisId> GetKey<T>(RedisId key)
		{
			return GetKeys<T>(new[] { key });
		}

		public override IEnumerable<RedisId> GetKeys<T>(IEnumerable<RedisId> keys)
		{
			return keys.Where(key => !string.IsNullOrWhiteSpace(key?.FullKey))
				.SelectMany(x => RedisService.Server(x).Keys(RedisService.Database.Database, x.FullKey))
				.Select(x => RedisService.CacheProvider.GetRedisId(x));
		}

		// not too slow, but could be problematic
		public override Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> allKeys)
		{
			IEnumerable<RedisId> keys = allKeys.Where(x => !string.IsNullOrWhiteSpace(x?.FullKey));
			// Lua uses 1-based arrays
			// Redis has the global "Table" (basically an array with generic indexing) KEYS for key arguments
			// https://www.lua.org/pil/2.5.html
			string luaScript = @"
					local keys = KEYS
					local retVal = ''
					local splitString = '|||||'
					local splitKvp = '^^^^^'
					local result = {}
					for i=1,#keys,1 do 
						local ttlthing = redis.call('ttl', keys[i])
						retVal = (retVal) .. (keys[i]) .. (splitKvp) .. ttlthing .. (splitString)
					end
					return retVal"; // retVal = (retVal) .. (splitString) //  .. (keys[i])
			RedisKey[] redisValues = keys.Select(x => (RedisKey)x.FullKey).ToArray();
			RedisResult result = RedisService.Database.ScriptEvaluate(luaScript, redisValues);
			string[] splitResult = result.ToString()
				.Split(new[] { "|||||" }, StringSplitOptions.RemoveEmptyEntries);
			Dictionary<RedisId, TimeSpan?> ttls = new Dictionary<RedisId, TimeSpan?>();
			foreach (string s in splitResult)
			{
				if (string.IsNullOrWhiteSpace(s))
				{
					continue;
				}

				string[] splitResult2 = s
					.Split(new[] { "^^^^^" }, StringSplitOptions.RemoveEmptyEntries);
				if (splitResult2.Length != 2)
				{
					continue;
				}

				RedisId key = keys.FirstOrDefault(k => k.FullKey.Contains(s));
				int ttlSeconds;
				bool gotTtlSeconds = int.TryParse(splitResult2[1], out ttlSeconds);
				if (!gotTtlSeconds)
				{
					ttls.Add(key, null);
				}
				ttls.Add(key, TimeSpan.FromSeconds(ttlSeconds));
			}

			return ttls;
		}

		protected override IEnumerable<RedisValue> ServiceGetValues<T>(IEnumerable<RedisId> keys)
		{
			RedisKey[] expandedKeys = keys.Select(x => (RedisKey)x.FullKey).ToArray();
			IEnumerable<RedisValue> results = RedisService.Database.StringGet(expandedKeys)
				.Where(x => x.HasValue);
			// List<RedisCachedObject<T>> values = new List<RedisCachedObject<T>>(expandedKeys.Length);
			// foreach (RedisValue result in results)
			// {
			// 	RedisCachedObject<T> current = RedisService.CacheProvider.GetCachedValue<T>(result, this);
			// 	if (current != null)
			// 	{
			// 		values.Add(current);
			// 	}
			// }

			// return values;
			return results;
		}

		protected override IEnumerable<RedisCachedObject<T>> ServiceValuePostProcess<T>(IEnumerable<RedisCachedObject<T>> results)
		{
			return results;
		}

		// slow and bad
		public override long KeyCount()
		{
			return GetKeys<object>(new[]
			{
				new RedisId
				{
					HashSetIdentifier = RedisService.CacheSettings.ServiceSettings.PrefixIdentifier,
					ObjectIdentifier = "*",
					ServiceType = RedisServiceType.KeyValue
				}
			}).LongCount();
		}
	}
}