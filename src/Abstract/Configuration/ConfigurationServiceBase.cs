using System;
using shoellibraries.Abstract.Caching;
using shoellibraries.Enums;

namespace shoellibraries.Abstract.Configuration
{
    public abstract class ConfigurationServiceBase : IConfigurationService
    {
        public ConfigurationServiceBase(ConfigurationCacheBase cachingService)
        {
            CacheBase = cachingService;
        }

        public virtual IConfigurationCache Cache => CacheBase;

        protected virtual ConfigurationCacheBase CacheBase { get; set; }

        public virtual void ResetConfigurations()
        {
            CacheBase.ResetCache();
        }
        
        public virtual void ClearConfiguration<T>(IConfigurationMetadata configurationMetadata = null)
            where T: IConfiguration
        {
            CacheBase.ClearConfiguration<T>(configurationMetadata);
        }

        public virtual T GetConfiguration<T>(IConfigurationMetadata configurationMetadata = null)
            where T: IConfiguration
        {
            return CacheBase.GetConfiguration<T>(configurationMetadata);
        }

        public virtual bool DoesConfigurationExist<T>(IConfigurationMetadata configurationMetadata = null)
            where T: IConfiguration
        {
            return CacheBase.DoesConfigurationExist<T>(configurationMetadata);
        }
    }
}