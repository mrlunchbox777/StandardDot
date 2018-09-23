using System;
using StandardDot.Caching.Redis.Dto;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Abstract
{
	public interface ICacheServiceSettings
	{
		Guid CacheProviderSettingsId { get; }

		TimeSpan? DefaultExpireTimeSpan { get; }

		int DefaultScanPageSize { get; set; }

		RedisServiceType RedisServiceImplementationType { get; set; }

		bool CompressValues { get; set; }

		string PrefixIdentifier { get; }

		CacheInfo ProviderInfo { get; set; }
		
		string ConfigurationOptions { get; set;}
	}
}