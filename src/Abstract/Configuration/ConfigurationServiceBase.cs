using System;
using StandardDot.Abstract.Caching;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
    public abstract class ConfigurationServiceBase : IConfigurationService
    {
        public ConfigurationServiceBase(ConfigurationCacheBase cachingService)
        {
            CacheBase = cachingService;
        }

        public virtual IConfigurationCache Cache => CacheBase;

        protected virtual ConfigurationCacheBase CacheBase { get; set; }

        public virtual void ResetCachedConfigurations()
        {
            CacheBase.ResetCache();
        }
        
        public virtual void ClearConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            CacheBase.ClearConfiguration<T, Tm>(configurationMetadata);
        }

        public virtual T GetConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            return CacheBase.GetConfiguration<T, Tm>(configurationMetadata);
        }

        public virtual bool DoesConfigurationExist<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            return CacheBase.DoesConfigurationExist<T, Tm>(configurationMetadata);
        }
    }
}