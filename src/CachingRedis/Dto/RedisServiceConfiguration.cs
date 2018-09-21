using System;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
	public class RedisServiceConfiguration : ICacheProviderSettings
	{
		public RedisServiceConfiguration(ISerializationService serializationService, ConfigurationOptions configurationOptions, Guid? Id = null)
		{
			ConfigurationOptions = configurationOptions;
			SerializationService = serializationService;
			CacheProviderSettingsId = Id ?? Guid.NewGuid();
		}

		public RedisServiceConfiguration(ISerializationService serializationService, string configurationOptionsString, Guid? Id = null)
			: this(serializationService, ConfigurationOptions.Parse(configurationOptionsString, true), Id)
		{ }

		public TimeSpan? DefaultExpireTimeSpan { get; set; } = TimeSpan.FromSeconds(3600);

		public int DefaultScanPageSize { get; set; } = 1000;

		public RedisServiceType RedisServiceImplementationType { get; set; } = RedisServiceType.HashSet;

		public bool CompressValues { get; set; } = false;

		public ConfigurationOptions ConfigurationOptions { get; set; }

		public ISerializationService SerializationService { get; set; }

		public Guid CacheProviderSettingsId { get; }
	}
}
