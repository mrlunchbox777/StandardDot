using System;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
	public class RedisServiceConfiguration : ICacheProviderSettings
	{
		public RedisServiceConfiguration(ISerializationService serializationService, ConfigurationOptions configurationOptions
			, CacheInfo providerInfo, Guid? Id = null)
		{
			ConfigurationOptions = configurationOptions;
			SerializationService = serializationService;
			CacheProviderSettingsId = Id ?? Guid.NewGuid();
			ProviderInfo = providerInfo;
		}

		public RedisServiceConfiguration(ISerializationService serializationService, string configurationOptionsString
			, CacheInfo providerInfo, Guid? Id = null)
			: this(serializationService, ConfigurationOptions.Parse(configurationOptionsString, true), providerInfo, Id)
		{ }

		public TimeSpan? DefaultExpireTimeSpan { get; set; } = TimeSpan.FromSeconds(3600);

		public int DefaultScanPageSize { get; set; } = 1000;

		public RedisServiceType RedisServiceImplementationType { get; set; } = RedisServiceType.HashSet;

		public bool CompressValues { get; set; } = false;

		public ConfigurationOptions ConfigurationOptions { get; set; }

		public ISerializationService SerializationService { get; set; }

		public Guid CacheProviderSettingsId { get; }

		public string PrefixIdentifier => (ProviderInfo?.CacheDomain ?? "") + (ProviderInfo?.CacheGroup ?? "");

		public CacheInfo ProviderInfo { get; set; }

		public IDataContractResolver DataContractResolver { get; set; }
	}
}
