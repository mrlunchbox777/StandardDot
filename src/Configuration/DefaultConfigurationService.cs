using System;
using StandardDot.Abstract.Configuration;

namespace Configuration
{
    public class DefaultConfigurationService : ConfigurationServiceBase
    {
        public DefaultConfigurationService(ConfigurationCacheBase cachingService) : base(cachingService)
        {
        }
    }
}
