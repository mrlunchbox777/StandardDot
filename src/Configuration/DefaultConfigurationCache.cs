using System;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.Configuration;
using StandardDot.Abstract.CoreServices;

namespace StandardDot.Configuration
{
    public class DefaultConfigurationCache : ConfigurationCacheBase
    {
        public DefaultConfigurationCache(ICachingService cachingService, ISerializationService serializationService,
            TimeSpan configurationLifeSpan)
            : base(cachingService, serializationService, configurationLifeSpan)
        {
        }
    }
}
