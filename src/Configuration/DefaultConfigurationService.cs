using System;
using StandardDot.Abstract.Configuration;

namespace StandardDot.Configuration
{
    public class DefaultConfigurationService : ConfigurationServiceBase
    {
        public DefaultConfigurationService(ConfigurationCacheBase cachingService) : base(cachingService)
        {
        }
    }
}
