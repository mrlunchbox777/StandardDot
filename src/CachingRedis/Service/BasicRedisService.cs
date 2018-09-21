using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
	internal class BasicRedisService : IRedisService
	{
		public BasicRedisService(RedisService redisService)
		{
			RedisService = redisService;
		}

		public RedisService RedisService { get; }

		public RedisServiceType ServiceType => RedisServiceType.KeyValue;

		public void AddToCache<T>(RedisHashSetCachedObject<T> source, IDataContractResolver dataContractResolver = null)
		{
			if ((source?.Id?.HasFullKey ?? false) || source.Value.Equals(default(T)))
			{
				return;
			}

			string compressedVal;
			if (source.Value is string)
			{
				compressedVal = source.Value as string;
			}
			else
			{
				ISerializationService sz = RedisService.GetSerializationService<T>(dataContractResolver);
				compressedVal = sz.SerializeObject<RedisHashSetCachedObject<T>>(source);
			}

			HardAddToCache(source.Id, compressedVal, source.ExpireTime);
		}

		public void DeleteFromCache(RedisId key)
		{
			RedisService.CacheProvider.DeleteValuesByKeys(new[] { key + "*" });
		}

		public void DeleteFromCache(IEnumerable<RedisId> keys)
		{
			foreach (RedisId key in keys)
			{
				DeleteFromCache(key);
			}
		}

		public T GetFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			if (!(key?.HasFullKey ?? false))
			{
				return default(T);
			}

			T retVal = default(T);
			string compressedVal = HardGetFromCache(key);
			if (!string.IsNullOrWhiteSpace(compressedVal))
			{
				retVal = RedisService.ConvertString<T>(compressedVal, key, dataContractResolver);
			}

			return retVal;
		}

		public IEnumerable<RedisId> GetKeys(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			if (!(key?.HasFullKey ?? false))
			{
				return new List<RedisId>();
			}

			return RedisService.Server(key.FullKey).Keys(RedisService.Database.Database, key.FullKey,
				RedisService.CacheSettings.DefaultScanPageSize)
				.Select(x => new RedisId { HashSetIdentifier = null, ObjectIdentifier = x, ServiceType = RedisServiceType.KeyValue });
		}

		public IEnumerable<string> GetKeysStrings(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			return GetKeys(key).Select(k => k.FullKey);
		}

		public IEnumerable<T> GetListFromCache<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null)
		{
			IEnumerable<Tuple<RedisId, string>> values = GetStringListFromCache<T>(keys, dataContractResolver);
			return values.Select(value => RedisService.ConvertString<T>(value.Item2, value.Item1, dataContractResolver));
		}

		public IEnumerable<T> GetListFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			return GetListFromCache<T>(new[] { key }, dataContractResolver);
		}

		private IEnumerable<Tuple<RedisId, string>> GetStringListFromCache<T>(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			return GetStringListFromCache<T>(new[] { key }, dataContractResolver);
		}

		// redis id is null on this
		private IEnumerable<Tuple<RedisId, string>> GetStringListFromCache<T>(IList<RedisId> keys, IDataContractResolver dataContractResolver = null)
		{
			if (!(keys?.Any() ?? false))
			{
				return new List<Tuple<RedisId, string>>();
			}

			for (int i = 0; i < keys.Count; i++)
			{
				keys[i].ObjectIdentifier = keys[i].ObjectIdentifier + "*";
			}

			// get the keys, in a non-blocking way
			List<RedisKey> redisKeys = new List<RedisKey>();
			foreach (RedisId key in keys)
			{
				redisKeys.AddRange(GetKeys(key).Select(x => (RedisKey)x.FullKey));
			}

			if (!redisKeys.Any())
			{
				return new List<Tuple<RedisId, string>>();
			}

			RedisValue[] results = RedisService.Database.StringGet(redisKeys.ToArray());

			IEnumerable<Tuple<RedisId, string>> values = results.Select(redisValue => new Tuple<RedisId, string>(null, (string)redisValue));

			return values;
		}

		public TimeSpan? GetTimeToLive<T>(RedisId key, IDataContractResolver dataContractResolver = null)
		{
			if (string.IsNullOrWhiteSpace(key?.FullKey))
			{
				return null;
			}
			return RedisService.Database.KeyTimeToLive(key.FullKey);
		}

		public Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IList<RedisId> allKeys, IDataContractResolver dataContractResolver = null)
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

		private void HardAddToCache(RedisId key, string value, DateTime expiration)
		{
			RedisService.Database.StringSet(key.FullKey, value, expiration - DateTime.UtcNow);
		}

		private string HardGetFromCache(RedisId key)
		{
			return RedisService.Database.StringGet(key.FullKey);
		}

	}
}

