using System;
using StackExchange.Redis;
using StandardDot.Abstract.CoreServices;
using StandardDot.Caching.Redis.Abstract;
using StandardDot.Caching.Redis.Enums;

namespace StandardDot.Caching.Redis.Dto
{
    public class RedisServiceConfiguration : ICacheProviderSettings
    {
        public RedisServiceConfiguration(ISerializationService serializationService, ConfigurationOptions configurationOptions)
        {
            ConfigurationOptions = configurationOptions;
            SerializationService = serializationService;
        }

        public RedisServiceConfiguration(ISerializationService serializationService, string configurationOptionsString)
        {
            ConfigurationOptions = ConfigurationOptions.Parse(configurationOptionsString, true);
            SerializationService = serializationService;
        }

        public TimeSpan? DefaultExpireTimeSpan { get; set; } = TimeSpan.FromSeconds(3600);
        
        public int DefaultScanPageSize { get; set; } = 1000;
        
        public RedisServiceType RedisServiceImplementationType { get; set; } = RedisServiceType.HashSet;

        public bool CompressValues { get; set; } = false;

        public ConfigurationOptions ConfigurationOptions { get; set; }

        public ISerializationService SerializationService { get; set; }
    }
}
