﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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

		public virtual RedisValue GetValueToCache<T>(RedisCachedObject<T> cachedObject)
		{
			RedisValue redisValue = CacheSettings.SerializationService.SerializeObject(cachedObject
				, CacheSettings.SerializationSettings);
			if (CacheSettings.ServiceSettings.CompressValues)
			{
				redisValue = CompressValue(redisValue);
			}
			return redisValue;
		}

		protected virtual RedisCachedObject<T> GetCachedValue<T>(RedisValue redisValue, IRedisService service, bool first)
		{
			string decompressed = null;
			if (default(RedisValue) == redisValue)
			{
				return CreateCachedValue<T>();
			}
			if (CacheSettings.ServiceSettings.CompressValues)
			{
				decompressed = DecompressValue(redisValue);
			}
			if (default(RedisValue) == redisValue)
			{
				return CreateCachedValue<T>();
			}
			decompressed = decompressed ?? redisValue;
			RedisCachedObject<T> value;
			if (!first)
			{
				ISerializationService sz = CacheSettings.SerializationService;
				try
				{
					return sz.DeserializeObject<RedisCachedObject<T>>(decompressed, CacheSettings.SerializationSettings);
				}
				catch (SerializationException)
				{
					return CreateCachedValue<T>();
				}
			}

			RedisCachedObject<string> stringValue = GetCachedValue<string>(redisValue, service, false);
			if (typeof(T) == typeof(object) || typeof(T) == typeof(string))
			{
				value = ChangeType<T, string>(stringValue);
			}
			else if (typeof(T).IsPrimitive)
			{
				value = ChangeType<T, string>(stringValue);
			}
			else
			{
				ISerializationService sz = CacheSettings.SerializationService;
				value = new RedisCachedObject<T>
				{
					RetrievedSuccesfully = stringValue.RetrievedSuccesfully,
					Value = sz.DeserializeObject<T>(stringValue.Value, CacheSettings.SerializationSettings),
					CachedTime = stringValue.CachedTime,
					Status = stringValue.Status,
					Metadata = stringValue.Metadata,
					Id = stringValue.Id,
					ExpireTime = stringValue.ExpireTime,
				};
			}

			if (value != null && value.ExpireTime < DateTime.UtcNow)
			{
				if (value.Id != null)
				{
					service.DeleteValue(value.Id);
				}
				return CreateCachedValue<T>();
			}

			return value;
		}

		public virtual RedisCachedObject<T> GetCachedValue<T>(RedisValue redisValue, IRedisService service)
		{
			return GetCachedValue<T>(redisValue, service, true);
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

		public virtual RedisId GetRedisId(RedisId key)
		{
			return key;
		}

		public virtual RedisId GetRedisId(string key, bool tryJson = true)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}
			if (tryJson && key.StartsWith("{") && key.EndsWith("}")
				&& RedisId.DataMemberNames.All(x => key.Contains(x)))
			{
				return key;
			}
			string adjustedKey = key;
			if (!string.IsNullOrWhiteSpace(CacheSettings.ServiceSettings.PrefixIdentifier)
				&& key.StartsWith(CacheSettings.ServiceSettings.PrefixIdentifier))
			{
				adjustedKey = key.Replace(CacheSettings.ServiceSettings.PrefixIdentifier, "");
			}
			while (adjustedKey.StartsWith(":"))
			{
				adjustedKey = adjustedKey.Remove(0, 1);
			}
			if (!string.IsNullOrWhiteSpace(CacheSettings.ServiceSettings.ProviderInfo.ObjectPrefix)
				&& key.StartsWith(CacheSettings.ServiceSettings.ProviderInfo.ObjectPrefix))
			{
				adjustedKey = adjustedKey.Replace(CacheSettings.ServiceSettings.ProviderInfo.ObjectPrefix, "");
			}
			while (adjustedKey.StartsWith(":"))
			{
				adjustedKey = adjustedKey.Remove(0, 1);
			}
			return new RedisId
			{
				ServiceType = CacheSettings.ServiceSettings.RedisServiceImplementationType,
				ObjectIdentifier = CacheSettings.ServiceSettings.ProviderInfo.ObjectPrefix + adjustedKey,
				HashSetIdentifier = CacheSettings.ServiceSettings.PrefixIdentifier
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

		public virtual byte[] CompressValue(RedisValue redisValue)
		{
			if (redisValue == default(RedisValue))
			{
				return default(RedisValue);
			}

			return ((string)redisValue).Zip();
		}

		public virtual RedisValue DecompressValue(byte[] redisValue)
		{
			if (redisValue == default(byte[]))
			{
				return default(RedisValue);
			}

			return redisValue.Unzip();
		}

		public virtual RedisCachedObject<T> ChangeType<T, TK>(RedisCachedObject<TK> source)
		{
			if (source == null)
			{
				return CreateCachedValue<T>();
			}
			RedisCachedObject<T> wrapper = new RedisCachedObject<T>
			{
				RetrievedSuccesfully = source.RetrievedSuccesfully,
				Value = (T)source.UntypedValue,
				CachedTime = source.CachedTime,
				Status = source.Status,
				Metadata = source.Metadata,
				Id = source.Id,
				ExpireTime = source.ExpireTime,
			};
			return wrapper;
		}
	}
}