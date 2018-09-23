using System;
using System.Collections.Generic;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.Configuration;
using StandardDot.Abstract.CoreServices;
using StandardDot.CoreServices.Serialization;
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
					_configurationService = new GenerateService()
				}
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
			Json jsonSerializer = new Json();
			MemoryCachingService cachingService = new MemoryCachingService(TimeSpan.FromDays(1), false);
			Mock<ConfigurationCacheBase> cacheProxy =
				new Mock<ConfigurationCacheBase>(useStrict ? MockBehavior.Strict : MockBehavior.Loose,
					cachingService, jsonSerializer, TimeSpan.FromDays(1));

			return cacheProxy;
		}

		internal static ICacheProviderSettings GetCacheProviderSettings()
		{
			RedisServiceConfiguration configuration = new RedisServiceConfiguration()
		}
	}
}