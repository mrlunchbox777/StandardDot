using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		public RedisService(ICacheProviderSettings settings, ILoggingService loggingService
			, Func<ICacheProviderSettings, ILoggingService, ICacheProvider> resetProvider)
		{
			_cacheSettings = settings;
			_loggingService = loggingService;
			ResetProvider = resetProvider;
		}

		private ICacheProviderSettings _cacheSettings;

		/// <summary>
		/// Set this before using anything
		/// </summary>
		public ICacheProviderSettings CacheSettings => _cacheSettings;

		private ILoggingService _loggingService;

		public ILoggingService LoggingService => _loggingService;

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

		protected virtual Func<ICacheProviderSettings, ILoggingService, ICacheProvider> ResetProvider { get; }

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
				return EnsureValidRedisServiceImplementation(CacheSettings.ServiceSettings.RedisServiceImplementationType);
			}
			set
			{

				if (_redisServiceImplementation.ContainsKey(CacheSettings.ServiceSettings.RedisServiceImplementationType))
				{
					_redisServiceImplementation[CacheSettings.ServiceSettings.RedisServiceImplementationType] = value;
				}
				_redisServiceImplementation.TryAdd(CacheSettings.ServiceSettings.RedisServiceImplementationType, value);
			}
		}

		protected virtual IRedisService EnsureValidRedisServiceImplementation(
			RedisServiceType redisServiceImplementationType)
		{
			if (_redisServiceImplementation.ContainsKey(CacheSettings.ServiceSettings.RedisServiceImplementationType)
				&& (_redisServiceImplementation[CacheSettings.ServiceSettings.RedisServiceImplementationType] != null))
			{
				return _redisServiceImplementation[CacheSettings.ServiceSettings.RedisServiceImplementationType];
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
				CacheProvider = ResetProvider(this.CacheSettings, this.LoggingService);
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
		/// <param name="serializationSettings">The datacontract resolver to use for serialization
		/// (polymorphic dtos)</param>
		/// <returns>Object from string</returns>
		protected internal virtual T ConvertString<T>(string stringToConvert, RedisId key,
			ISerializationSettings serializationSettings = null)
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
				retVal = serializationService.DeserializeObject<T>(stringToConvert, _cacheSettings.SerializationSettings);
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

		public virtual RedisServiceType ActiveServiceType => CacheSettings.ServiceSettings.RedisServiceImplementationType;

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

		public virtual long DeleteValues(IEnumerable<RedisId> keys)
		{
			return GetRedisServiceImplementation().DeleteValues(keys);
		}

		public virtual bool DeleteValue(RedisId key)
		{
			return GetRedisServiceImplementation().DeleteValue(key);
		}

		public virtual long KeyCount()
		{
			return GetRedisServiceImplementation().KeyCount();
		}

		public virtual Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(RedisId key)
		{
			return GetRedisServiceImplementation().GetTimeToLive<T>(key);
		}

		public virtual Dictionary<RedisId, TimeSpan?> GetTimeToLive<T>(IEnumerable<RedisId> keys)
		{
			return GetRedisServiceImplementation().GetTimeToLive<T>(keys);
		}

		public long ContainsKeys(IEnumerable<RedisId> keys)
		{
			return GetRedisServiceImplementation().ContainsKeys(keys);
		}

		public bool ContainsKey(RedisId key)
		{
			return GetRedisServiceImplementation().ContainsKey(key);
		}
	}
}