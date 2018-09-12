using System;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Abstract
{
    public interface ICacheProviderSettings
    {
        Guid CacheProviderSettingsId { get; }

        TimeSpan? DefaultExpireTimeSpan { get; set; }
        
        int DefaultScanPageSize { get; set; }
        
        RedisServiceType RedisServiceImplementationType { get; set; }

        bool CompressValues { get; set; }

        ConfigurationOptions ConfigurationOptions { get; set; }

        ISerializationService SerializationService { get; set; }
    }
}