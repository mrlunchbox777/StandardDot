using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;
using StandardDot.CoreExtensions;

namespace StandardDot.Caching.Redis.Providers
{
	internal class RedisCacheProvider : ICacheProvider, IDisposable
	{
		protected StringWriter ConnectLog;

		protected ILoggingService Logger;

		protected ConnectionMultiplexer Redis;

		public RedisCacheProvider(ICacheProviderSettings settings, ILoggingService logger)
		{
			CacheSettings = settings;
			Logger = logger;
		}

		public ICacheProviderSettings CacheSettings { get; set; }

		public IDatabase Database { get; set; }

		protected virtual void Initialize()
		{
			// use the logger instead
			ConnectLog = new StringWriter();
			Redis = ConnectionMultiplexer.Connect(
				CacheSettings.ConfigurationOptions, ConnectLog);
		}

		// Abstract implementation

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

		public void Dispose()
		{
			ConnectLog.Dispose();
		}

		// Protected

		protected virtual string CompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return Convert.ToBase64String(redisValue.Zip());
		}

		protected virtual string DecompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return redisValue.Unzip();
		}
	}
}