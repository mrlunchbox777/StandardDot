using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;
using StandardDot.Caching.Redis.Providers;

namespace StandardDot.Caching.Redis.Service
{
	internal class RedisService : IRedisService
	{
		public RedisService(ICacheProviderSettings settings, Func<ICacheProviderSettings, ICacheProvider> resetProvider)
		{
			_cacheSettings = settings;
			ResetProvider = resetProvider;
		}

		private ICacheProviderSettings _cacheSettings;

		/// <summary>
		/// Set this before using anything
		/// </summary>
		public ICacheProviderSettings CacheSettings => _cacheSettings;

		private static ICacheProvider _cacheProvider;

		internal ICacheProvider CacheProvider
		{
			get
			{
				if (_cacheProvider == null)
				{
					ResetCache();
				}
				return _cacheProvider;
			}
			private set { _cacheProvider = value; }
		}

		protected virtual Func<ICacheProviderSettings, ICacheProvider> ResetProvider { get; }

		private static ConnectionMultiplexer _redis;

		protected ConnectionMultiplexer Redis
		{
			get
			{
				if (_redis != null && _redis.IsConnected)
				{
					return _redis;
				}

				ResetCache();
				_redis = CacheProvider?.GetRedis();
				return _redis;
			}
		}

		private static IDatabase _db;

		protected internal virtual IDatabase Database
		{
			get
			{
				_db = Redis?.GetDatabase();
				return _db;
			}
		}

		private static IServer _server;

		protected internal virtual IServer Server(string key)
		{
			_server = Redis?.GetServer(Database.IdentifyEndpoint(key));
			return _server;
		}

		protected static readonly ConcurrentDictionary<RedisServiceType, IRedisService> _redisServiceImplementation =
			new ConcurrentDictionary<RedisServiceType, IRedisService>();

		/// <summary>
		/// If you are going to set this, make sure that you change the redisserviceimplementationtype first
		/// </summary>
		public virtual IRedisService RedisServiceImplementation
		{
			get
			{
				return EnsureValidRedisServiceImplementation(CacheSettings.RedisServiceImplementationType);
			}
			set
			{

				if (_redisServiceImplementation.ContainsKey(CacheSettings.RedisServiceImplementationType))
				{
					_redisServiceImplementation[CacheSettings.RedisServiceImplementationType] = value;
				}
				_redisServiceImplementation.TryAdd(CacheSettings.RedisServiceImplementationType, value);
			}
		}

		protected virtual IRedisService EnsureValidRedisServiceImplementation(
			RedisServiceType redisServiceImplementationType)
		{
			if (_redisServiceImplementation.ContainsKey(CacheSettings.RedisServiceImplementationType)
				&& (_redisServiceImplementation[CacheSettings.RedisServiceImplementationType] != null))
			{
				return _redisServiceImplementation[CacheSettings.RedisServiceImplementationType];
			}

			IRedisService helper;
			switch (redisServiceImplementationType)
			{
				case RedisServiceType.HashSet:
					helper = new HashSetRedisService(this);
					break;
				case RedisServiceType.KeyValue:
					helper = new BasicRedisService(this);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(redisServiceImplementationType));
			}

			if (_redisServiceImplementation.ContainsKey(redisServiceImplementationType))
			{
				_redisServiceImplementation[redisServiceImplementationType] = helper;
			}
			else
			{
				_redisServiceImplementation.TryAdd(redisServiceImplementationType, helper);
			}
			return _redisServiceImplementation[redisServiceImplementationType];
		}

		protected virtual IRedisService GetRedisServiceImplementation(RedisServiceType? type = null)
		{
			if (type == null)
			{
				return RedisServiceImplementation;
			}

			return EnsureValidRedisServiceImplementation((RedisServiceType)type);
		}


		protected virtual void ResetCache(RedisCacheProvider provider = null, bool forceReset = false)
		{
			if (provider == null || forceReset)
			{
				CacheProvider = ResetProvider(this.CacheSettings);
			}
			else
			{
				CacheProvider = provider;
			}
		}

		/// <summary>
		/// Converts a string found in cache to an object
		/// </summary>
		/// <typeparam name="T">Object type to convert to</typeparam>
		/// <param name="stringToConvert">String to convert to an object</param>
		/// <param name="key">The key that was used to get the value</param>
		/// <param name="dataContractResolver">The datacontract resolver to use for serialization
		/// (polymorphic dtos)</param>
		/// <returns>Object from string</returns>
		protected internal virtual T ConvertString<T>(string stringToConvert, RedisId key,
			IDataContractResolver dataContractResolver = null)
		{
			T retVal = default(T);

			if (string.IsNullOrWhiteSpace(stringToConvert))
			{
				return retVal;
			}

			if (typeof(T) == typeof(string))
			{
				retVal = (T)Convert.ChangeType(stringToConvert, typeof(T));
			}
			else
			{
				ISerializationService serializationService = GetSerializationService<T>();
				retVal = serializationService.DeserializeObject<T>(stringToConvert);
			}

			return retVal;
		}

		protected internal virtual ISerializationService GetSerializationService<T>()
		{
			// use data contract resolver
			return CacheSettings.SerializationService;
		}

		// Abstract Implementation

		public virtual RedisServiceType ServiceType => RedisServiceType.Provider;

		public virtual RedisServiceType ActiveServiceType => CacheSettings.RedisServiceImplementationType;

		public virtual IEnumerable<RedisId> GetKeys<T>(IEnumerable<RedisId> keys)
		{
			return GetRedisServiceImplementation().GetKeys<T>(keys);
		}

		public virtual IEnumerable<RedisId> GetKey<T>(RedisId key)
		{
			return GetRedisServiceImplementation().GetKey<T>(key);
		}

		public virtual IEnumerable<RedisCachedObject<T>> GetValues<T>(IEnumerable<RedisId> keys)
		{
			return GetRedisServiceImplementation().GetValues<T>(keys);
		}

		public virtual IEnumerable<RedisCachedObject<T>> GetValue<T>(RedisId key)
		{
			return GetRedisServiceImplementation().GetValue<T>(key);
		}

		public virtual IEnumerable<RedisCachedObject<T>> SetValues<T>(IEnumerable<RedisCachedObject<T>> values)
		{
			return GetRedisServiceImplementation().SetValues<T>(values);
		}

		public virtual IEnumerable<RedisCachedObject<T>> SetValue<T>(RedisCachedObject<T> value)
		{
			return GetRedisServiceImplementation().SetValue<T>(value);
		}

		public virtual void DeleteValues(IEnumerable<RedisId> keys)
		{
			GetRedisServiceImplementation().DeleteValues(keys);
		}

		public virtual void DeleteValue(RedisId key)
		{
			GetRedisServiceImplementation().DeleteValue(key);
		}

		public virtual long KeyCount()
		{
			return GetRedisServiceImplementation().KeyCount();
		}

		public virtual Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(RedisId key)
		{
			throw new NotImplementedException();
		}

		public virtual Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> keys)
		{
			throw new NotImplementedException();
		}
	}
}

/*
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
 */
