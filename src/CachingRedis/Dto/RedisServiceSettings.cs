using System;
using System.Runtime.Serialization;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
	[DataContract]
	public class RedisServiceSettings : ICacheServiceSettings
	{
		public RedisServiceSettings()
		{
			ProviderInfo = ProviderInfo ?? new CacheInfo();
		}

		[DataMember(Name = "configurationOptions")]
		public string ConfigurationOptions { get; set; }

		[DataMember(Name = "defaultScanPageSize")]
		public int DefaultScanPageSize { get; set; } = 1000;

		[DataMember(Name = "redisServiceImplementationType")]
		public RedisServiceType RedisServiceImplementationType { get; set; } = RedisServiceType.HashSet;

		[DataMember(Name = "compressValues")]
		public bool CompressValues { get; set; } = false;

		[DataMember(Name = "providerInfo")]
		public CacheInfo ProviderInfo { get; set; }

		[DataMember(Name = "cacheProviderSettingsId")]
		public string CacheProviderSettingsIdString { get; set; }

		/// <summary>
		/// The default expire time, use seconds
		/// </summary>
		[DataMember(Name = "defaultExpireTimeSpan")]
		public int DefaultExpireTimeSpanSeconds { get; set; } = 3600;

		[IgnoreDataMember]
		public Guid CacheProviderSettingsId => Guid.Parse(CacheProviderSettingsIdString);
		
		[IgnoreDataMember]
		public TimeSpan? DefaultExpireTimeSpan => TimeSpan.FromSeconds(DefaultExpireTimeSpanSeconds);

		[IgnoreDataMember]
		public string PrefixIdentifier => (ProviderInfo?.CacheDomain ?? "") + (ProviderInfo?.CacheGroup ?? "");
	}
}