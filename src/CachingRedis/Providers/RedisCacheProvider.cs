using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StackExchange.Redis;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;
using StandardDot.CoreExtensions;

namespace StandardDot.Caching.Redis.Providers
{
	internal class RedisCacheProvider : ICacheProvider
	{
		protected StringWriter ConnectLog;

		protected ConnectionMultiplexer Redis;

		public RedisCacheProvider(ICacheProviderSettings settings)
		{
			CacheSettings = settings;
		}

		public ICacheProviderSettings CacheSettings { get; set; }

		public IDatabase Database { get; set; }

		public virtual List<RedisCachedObject<T>> GetValuesByKeys<T>(string[] keys)
		{
			if (keys == null)
			{
				return new List<RedisCachedObject<T>>();
			}

			keys = keys.Where(k => !string.IsNullOrWhiteSpace(k)).ToArray();
			IDatabase db = GetDatabase();
			List<RedisCachedObject<T>> results = new List<RedisCachedObject<T>>();
			foreach (string key in keys)
			{
				RedisValue result = db.StringGet(key);
				// deserialize the value
				if (result.HasValue)
				{
					RedisCachedObject<T> value = GetCachedValue<T>(result);
					value.RetrievedSuccesfully = true;
					value.Status = CacheEntryStatus.Success;
					results.Add(value);
				}
				else
				{
					results.Add(new RedisCachedObject<T>(key)
					{
						Value = default(T),
						RetrievedSuccesfully = false,
						Status = CacheEntryStatus.Error,
					});
				}
			}

			return results;
		}

		public virtual RedisCachedObject<T> SetValue<T>(RedisCachedObject<T> value)
		{
			return SetValues(new[] { value }).SingleOrDefault();
		}

		public virtual List<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> valuesToCache)
		{
			if (valuesToCache == null)
			{
				return new List<RedisCachedObject<T>>();
			}

			List<RedisCachedObject<T>> values = valuesToCache.Where(v => !(v?.Id?.HasFullKey ?? false)).ToList();
			IDatabase db = GetDatabase();
			foreach (RedisCachedObject<T> entry in values)
			{
				// get the expiration, key and value
				entry.CachedTime = DateTime.UtcNow;
				string valueToStore = GetValueToCache(entry);
				bool success = db.StringSet(entry.Id.FullKey, valueToStore, entry.ExpireTime - entry.CachedTime);
				entry.Status = success ? CacheEntryStatus.Success : CacheEntryStatus.Error;
			}

			return values;
		}

		public virtual void DeleteValue(string prefix, string key)
		{
			IDatabase db = GetDatabase();
			db.HashDelete(prefix, key);
		}

		public virtual void DeleteValuesByKeys(string[] keys)
		{
			//remove all keys that are null, empty or set to "*"
			string[] keysToExpire = (keys == null ? new string[] { } : keys)
				.Select(k => (k ?? "").Trim()).Where(k => k != "*" && k != "").ToArray();

			IDatabase db = GetDatabase();
			foreach (string key in keysToExpire)
			{
				if (key.Last() == '*')
				{
					string luaScript = @"
					local keys = redis.call('keys', ARGV[1]) 
					for i=1,#keys,5000 do 
						redis.call('del', unpack(keys, i, math.min(i+4999, #keys))) 
					end 
					return keys";
					RedisResult result = db.ScriptEvaluate(luaScript, null, new RedisValue[] { key });
				}
				else
				{
					db.KeyDelete(key);
				}
			}
		}

		protected virtual void Initialize()
		{
			ConnectLog = new StringWriter();
			Redis = ConnectionMultiplexer.Connect(
				CacheSettings.ConfigurationOptions, ConnectLog);
		}

		public ConnectionMultiplexer GetRedis()
		{
			if (Redis == null)
			{
				Initialize();
			}
			return Redis;
		}

		public IDatabase GetDatabase()
		{
			if (Database != null)
			{
				return Database;
			}

			return GetRedis().GetDatabase();
		}

		public virtual string GetValueToCache<T>(RedisCachedObject<T> cachedObject)
		{
			string redisValue = CacheSettings.SerializationService.SerializeObject(cachedObject);
			if (CacheSettings.CompressValues)
			{
				redisValue = CompressValue(redisValue);
			}
			return redisValue;
		}

		public virtual RedisCachedObject<T> GetCachedValue<T>(string redisValue)
		{
			if (CacheSettings.CompressValues)
			{
				redisValue = DecompressValue(redisValue);
			}
			return CacheSettings.SerializationService.DeserializeObject<RedisCachedObject<T>>(redisValue);
			;
		}

		public static string CompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return Convert.ToBase64String(redisValue.Zip());
		}

		public static string DecompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return redisValue.Unzip();
		}

		public List<RedisCachedObject<T>> GetValuesByKeys<T>(IEnumerable<RedisId> keys)
		{
			throw new NotImplementedException();
		}

		public List<RedisCachedObject<T>> GetValue<T>(RedisId[] key)
		{
			throw new NotImplementedException();
		}

		List<RedisCachedObject<T>> ICacheProvider.SetValue<T>(RedisCachedObject<T> value)
		{
			throw new NotImplementedException();
		}

		public void DeleteValues(IEnumerable<string> keys)
		{
			throw new NotImplementedException();
		}
	}
}