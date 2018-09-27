using System;
using System.Collections.Generic;
using Moq;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.Configuration;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.UnitTests.Configuration;
using StandardDot.CoreServices.Serialization;
using StandardDot.Dto.CoreServices;
using StandardDot.TestClasses;

namespace StandardDot.Caching.Redis.UnitTests
{
	public static class RedisHelpers
	{
		internal static Random Random { get; } = new Random();

		internal static Foobar GetCachableObject()
		{
			Foobar cachable = new Foobar
			{
				Foo = Random.Next(-1000, 1000),
				Bar = Random.Next(-1000, 1000)
			};
			return cachable;
		}

		internal static string CachableKey => Random.Next(100, 100).ToString();

		private static ISerializationService _serializationService;
		internal static ISerializationService SerializationService
		{
			get
			{
				if (_serializationService == null)
				{
					_serializationService = new Json();
				}
				return _serializationService;
			}
		}

		private static IConfigurationService _configurationService;
		internal static IConfigurationService ConfigurationService
		{
			get
			{
				if (_configurationService == null)
				{
					_configurationService = GenerateService(GenerateCache(false)).Object;
				}
				return _configurationService;
			}
		}

		internal static KeyValuePair<string, ICachedObjectBasic> GetCachableKvp(DateTime originalTime,
			TimeSpan cacheLifeTime, Foobar cachable, string cachableKey)
		{
			return new KeyValuePair<string, ICachedObjectBasic>
			(
				cachableKey,
				new DefaultCachedObject<object>
				{
					Value = cachable,
					ExpireTime = originalTime.Add(cacheLifeTime),
					CachedTime = originalTime
				}
			);
		}

		internal static Mock<ConfigurationServiceBase> GenerateService(Mock<ConfigurationCacheBase> cacheProxy)
		{
			Mock<ConfigurationServiceBase> configurationServiceProxy
				= new Mock<ConfigurationServiceBase>(MockBehavior.Loose, cacheProxy.Object);
			configurationServiceProxy.CallBase = true;
			return configurationServiceProxy;
		}

		private static Mock<ConfigurationCacheBase> GenerateCache(bool useStrict = true)
		{
			MemoryCachingService cachingService = new MemoryCachingService(TimeSpan.FromDays(1), false);
			Mock<ConfigurationCacheBase> cacheProxy =
				new Mock<ConfigurationCacheBase>(useStrict ? MockBehavior.Strict : MockBehavior.Loose,
					cachingService, SerializationService, TimeSpan.FromDays(1));
			cacheProxy.CallBase = !useStrict;

			return cacheProxy;
		}

		internal static Mock<LoggingServiceBase> GenerateLoggingService()
		{
			Mock<LoggingServiceBase> serviceProxy = new Mock<LoggingServiceBase>(MockBehavior.Loose, SerializationService);
			serviceProxy.CallBase = true;

			return serviceProxy;
		}

		private static ISerializationSettings _serializationSettings;

		internal static ISerializationSettings SerializationSettings
		{
			get
			{
				if (_serializationSettings == null)
				{
					_serializationSettings = new TestSerializationSettings();
				}
				return _serializationSettings;
			}
		}

		internal static ICacheProviderSettings GetCacheProviderSettings(bool compressValues = false)
		{
			RedisServiceSettings settings = null;
			if (compressValues)
			{
				TestRedisConfiguration config = ConfigurationService.GetConfiguration<TestRedisConfiguration, TestRedisConfigurationMetadata>();
				settings = config.RedisSettings;
			}
			else
			{
				TestRedisConfigurationNoCompress config = ConfigurationService.GetConfiguration<TestRedisConfigurationNoCompress, TestRedisConfigurationMetadataNoCompress>();
				settings = config.RedisSettings;
			}
			RedisProviderSettings configuration = new RedisProviderSettings(SerializationService, settings, SerializationSettings);
			return configuration;
		}

		internal static RedisCachingService GetRedis(bool compressValues = false)
		{
			return new RedisCachingService(GetCacheProviderSettings(compressValues), GenerateLoggingService().Object);
		}
	}
}