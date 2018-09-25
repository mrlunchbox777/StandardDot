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
			string redisValue = CacheSettings.SerializationService.SerializeObject(cachedObject
				, CacheSettings.SerializationSettings);
			if (CacheSettings.ServiceSettings.CompressValues)
			{
				redisValue = CompressValue(redisValue);
			}
			return redisValue;
		}

		public virtual RedisCachedObject<T> GetCachedValue<T>(string redisValue, IRedisService service)
		{
			if (string.IsNullOrWhiteSpace(redisValue))
			{
				return null;
			}
			if (CacheSettings.ServiceSettings.CompressValues)
			{
				redisValue = DecompressValue(redisValue);
			}
			if (string.IsNullOrWhiteSpace(redisValue))
			{
				return null;
			}
			RedisCachedObject<T> value = CacheSettings.SerializationService
				.DeserializeObject<RedisCachedObject<T>>(redisValue, CacheSettings.SerializationSettings);

			if (value != null && value.ExpireTime < DateTime.UtcNow)
			{
				if (value.Id != null)
				{
					service.DeleteValue(value.Id);
				}
				value = null;
			}

			return value;
		}

		public virtual RedisCachedObject<T> CreateCachedValue<T>(RedisId redisKey = null)
		{
			return new RedisCachedObject<T>(redisKey?.ObjectIdentifier)
			{
				RetrievedSuccesfully = false,
				Value = default(T),
				CachedTime = DateTime.MinValue,
				Status = CacheEntryStatus.Error,
				Metadata = CacheSettings.ServiceSettings.ProviderInfo,
				Id = redisKey,
				ExpireTime = DateTime.MinValue,
			};
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

		public virtual string CompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return Convert.ToBase64String(redisValue.Zip());
		}

		public virtual string DecompressValue(string redisValue)
		{
			if (redisValue == null)
			{
				return null;
			}

			return Convert.FromBase64String(redisValue).Unzip();
		}
	}
}