using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Abstract.DataStructures;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Service
{
	/// <summary>
	/// Check redis service for summaries
	/// </summary>
	internal class BasicRedisService : IRedisService
	{
		public BasicRedisService(RedisService redisService)
		{
			RedisService = redisService;
		}

		public RedisService RedisService { get; }

		public RedisServiceType ServiceType => RedisServiceType.KeyValue;

		protected virtual void HardAddToCache(RedisId key, string value, DateTime expiration)
		{
			if (RedisService.CacheSettings.ServiceSettings.CompressValues)
			{
				RedisService.Database.StringSet(key.FullKey, RedisService.CacheProvider.CompressValue(value), expiration - DateTime.UtcNow);
			}
			else
			{
				RedisService.Database.StringSet(key.FullKey, value, expiration - DateTime.UtcNow);
			}
		}

		protected virtual string HardGetFromCache(RedisId key)
		{
			RedisValue rawValue = RedisService.Database.StringGet(key.FullKey);
			RedisValue value = (default(RedisValue) != rawValue) && RedisService.CacheSettings.ServiceSettings.CompressValues
				? RedisService.CacheProvider.DecompressValue(rawValue)
				: (string)rawValue;
			return value;
		}

		// Abstract Implementation

		public IEnumerable<RedisId> GetKeys<T>(IEnumerable<RedisId> keys)
		{
			return keys.Where(key => !string.IsNullOrWhiteSpace(key?.FullKey) && RedisService.Database.KeyExists(key.FullKey));
		}

		public IEnumerable<RedisId> GetKey<T>(RedisId key)
		{
			return GetKeys<T>(new[] { key });
		}

		public IEnumerable<RedisCachedObject<T>> GetValues<T>(IEnumerable<RedisId> keys)
		{
			if (!(keys?.Any() ?? false))
			{
				return new[] { RedisService.CacheProvider.CreateCachedValue<T>() };
			}

			RedisId[] keysToUse = keys.ToArray();
			for (int i = 0; i < keysToUse.Count(); i++)
			{
				keysToUse[i].ObjectIdentifier = keysToUse[i].ObjectIdentifier + "*";
			}

			// get the keys, in a non-blocking way
			List<RedisKey> redisKeys = new List<RedisKey>();
			redisKeys.AddRange(GetKeys<T>(keysToUse).Select(x => (RedisKey)x.FullKey));

			if (!redisKeys.Any())
			{
				return new[] { RedisService.CacheProvider.CreateCachedValue<T>() };
			}

			RedisValue[] results = RedisService.Database.StringGet(redisKeys.ToArray());
			List<RedisCachedObject<T>> values = new List<RedisCachedObject<T>>(results.Length);
			foreach (RedisValue result in results)
			{
				RedisCachedObject<T> current = RedisService.CacheProvider.GetCachedValue<T>(result, this);
				if (current != null)
				{
					values.Add(current);
				}
			}

			return values;
		}

		public IEnumerable<RedisCachedObject<T>> GetValue<T>(RedisId key)
		{
			return GetValues<T>(new[] { key });
		}

		public IEnumerable<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> values)
		{
			RedisCachedObject<T>[] valuesEnumerated = values.ToArray();
			for (int i = 0; i < valuesEnumerated.Length; i++)
			{
				valuesEnumerated[i] = SetValue(valuesEnumerated[i]).SingleOrDefault();
			}
			return valuesEnumerated;
		}

		public IEnumerable<RedisCachedObject<T>> SetValue<T>(RedisCachedObject<T> value)
		{
			if ((value?.Id?.HasFullKey ?? false) || value.Value.Equals(default(T)))
			{
				return null;
			}

			string compressedVal;
			if (value.Value is string)
			{
				compressedVal = value.Value as string;
			}
			else
			{
				ISerializationService sz = RedisService.GetSerializationService<T>();
				compressedVal = sz.SerializeObject<RedisCachedObject<T>>(value, RedisService.CacheSettings.SerializationSettings);
			}

			HardAddToCache(value.Id, compressedVal, value.ExpireTime);
			return new[] { value };
		}

		public long DeleteValues(IEnumerable<RedisId> keys)
		{
			long deleteCount = 0;
			foreach (RedisId key in keys)
			{
				if (DeleteValue(key))
				{
					deleteCount++;
				}
			}
			return deleteCount;
		}

		public bool DeleteValue(RedisId key)
		{
			if (string.IsNullOrWhiteSpace(key?.FullKey))
			{
				return false;
			}
			return RedisService.Database.KeyDelete(key.FullKey);
		}

		// slow and bad
		public long KeyCount()
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

		// not too slow, but could be problematic
		public Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> allKeys)
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

		public Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(RedisId key)
		{
			return GetTimeToLive<T>(new[] { key });
		}

		public long ContainsKeys(IEnumerable<RedisId> keys)
		{
			if (!(keys?.Any() ?? false))
			{
				return 0;
			}
			return RedisService.Database.KeyExists(keys.Select(x => (RedisKey)((string)x)).ToArray());
		}

		public bool ContainsKey(RedisId key)
		{
			if (key == null)
			{
				return false;
			}
			return RedisService.Database.KeyExists(key.FullKey);
		}
	}
}

