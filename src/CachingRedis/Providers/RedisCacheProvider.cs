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

		// Abstract implementation

		// basic, needs to use get list thing
		public virtual List<RedisCachedObject<T>> GetValues<T>(IEnumerable<RedisId> keys)
		{
			if (keys == null)
			{
				return new List<RedisCachedObject<T>>();
			}

			keys = keys.Where(k => !string.IsNullOrWhiteSpace(k?.FullKey)).ToArray();
			IDatabase db = GetDatabase();
			List<RedisCachedObject<T>> results = new List<RedisCachedObject<T>>();
			foreach (RedisId key in keys)
			{
				RedisValue result = db.StringGet(key.FullKey);
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
					results.Add(new RedisCachedObject<T>(key.FullKey)
					{
						Value = default(T),
						RetrievedSuccesfully = false,
						Status = CacheEntryStatus.Error,
					});
				}
			}

			return results;
		}

		// not implemented
		public List<RedisCachedObject<T>> GetValue<T>(RedisId key)
		{
			throw new NotImplementedException();
		}

		// basic
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

		// single call of list
		public virtual RedisCachedObject<T> SetValue<T>(RedisCachedObject<T> value)
		{
			return SetValues(new[] { value }).SingleOrDefault();
		}

		// basic
		public virtual void DeleteValues(IEnumerable<RedisId> keys)
		{
			//remove all keys that are null, empty or set to "*"
			string[] keysToExpire = (keys == null ? new RedisId[] { } : keys)
				.Select(k => (k?.FullKey ?? "").Trim()).Where(k => k != "*" && k != "").ToArray();

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

		// hashset
		public virtual void DeleteValue(RedisId key)
		{
			IDatabase db = GetDatabase();
			db.HashDelete(key.HashSetIdentifier, key.ObjectIdentifier);
		}

		// not implemented
		public int KeyCount()
		{
			throw new NotImplementedException();
		}

		// Protected

		protected virtual void Initialize()
		{
			ConnectLog = new StringWriter();
			Redis = ConnectionMultiplexer.Connect(
				CacheSettings.ConfigurationOptions, ConnectLog);
		}

		protected virtual string GetValueToCache<T>(RedisCachedObject<T> cachedObject)
		{
			string redisValue = CacheSettings.SerializationService.SerializeObject(cachedObject);
			if (CacheSettings.CompressValues)
			{
				redisValue = CompressValue(redisValue);
			}
			return redisValue;
		}

		protected virtual RedisCachedObject<T> GetCachedValue<T>(string redisValue)
		{
			if (CacheSettings.CompressValues)
			{
				redisValue = DecompressValue(redisValue);
			}
			return CacheSettings.SerializationService.DeserializeObject<RedisCachedObject<T>>(redisValue);
			;
		}

		protected static string CompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return Convert.ToBase64String(redisValue.Zip());
		}

		protected static string DecompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return redisValue.Unzip();
		}

		List<RedisCachedObject<T>> ICacheProvider.SetValue<T>(RedisCachedObject<T> value)
		{
			throw new NotImplementedException();
		}
	}
}