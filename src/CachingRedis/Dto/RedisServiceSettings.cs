using System;
using System.Collections.Concurrent;
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

		[DataMember(Name = "redisServiceImplementationType")]
		public string RedisServiceImplementationTypeString { get; set; } = "HashSet";

		[IgnoreDataMember]
		public RedisServiceType RedisServiceImplementationType => (RedisServiceType)Enum.Parse(typeof(RedisServiceType), RedisServiceImplementationTypeString);
		
		[IgnoreDataMember]
		private static ConcurrentDictionary<string, Guid> _idTracker
			= new ConcurrentDictionary<string, Guid>();

		[IgnoreDataMember]
		public Guid CacheProviderSettingsId
		{
			get
			{
				if (_idTracker.ContainsKey(CacheProviderSettingsIdString))
				{
					return _idTracker[CacheProviderSettingsIdString];
				}
				bool gotAValue = Guid.TryParse(CacheProviderSettingsIdString, out Guid guidValue);
				if (gotAValue)
				{
					_idTracker[CacheProviderSettingsIdString] = guidValue;
					return guidValue;
				}
				guidValue = Guid.NewGuid();
				_idTracker[CacheProviderSettingsIdString] = guidValue;
				return guidValue;
			}
		}
		
		[IgnoreDataMember]
		public TimeSpan? DefaultExpireTimeSpan => TimeSpan.FromSeconds(DefaultExpireTimeSpanSeconds);

		[IgnoreDataMember]
		public string PrefixIdentifier => (ProviderInfo?.CacheDomain ?? "") + (ProviderInfo?.CacheGroup ?? "");
	}
}