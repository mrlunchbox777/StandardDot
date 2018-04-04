using System;
using System.IO;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
    public abstract class ConfigurationCacheBase : IConfigurationCache
    {
        public ConfigurationCacheBase(ICachingService cachingService, ISerializationService serializationService,
            TimeSpan configurationLifeSpan)
        {
            CachingService = cachingService;
            SerializationService = serializationService;
            ConfigurationLifeSpan = configurationLifeSpan;
        }

        protected virtual ICachingService CachingService { get; }

        protected virtual ISerializationService SerializationService { get; }

        protected virtual TimeSpan ConfigurationLifeSpan { get; }

        protected virtual TimeSpan MetadataLifeSpan { get; }

        public virtual int NumberOfConfigurations => CachingService.Count;

        public virtual void ResetCache()
        {
            CachingService.Clear();
        }

        protected internal virtual void ClearConfiguration<T>(IConfigurationMetadata configurationMetadata = null)
            where T: IConfiguration
        {
            IConfigurationMetadata metadata = GetMetadataForConfig<T>(configurationMetadata);
            if (CachingService.ContainsKey(metadata.ConfigurationName))
            {
                CachingService.Remove(metadata.ConfigurationName);
            }
        }

        protected internal virtual bool DoesConfigurationExist<T>(IConfigurationMetadata configurationMetadata = null)
            where T: IConfiguration
        {
            IConfigurationMetadata metadata = GetMetadataForConfig<T>(configurationMetadata);
            return CachingService.ContainsKey(metadata.ConfigurationName);
        }

        protected internal virtual void AddConfiguration<T>(T configuration)
            where T: IConfiguration
        {
            DateTime cacheTime = DateTime.UtcNow;
            ClearConfiguration<T>(configuration.ConfigurationMetadata);
            CachingService.Cache(configuration.ConfigurationMetadata.ConfigurationName, configuration, cacheTime, (cacheTime.Add(ConfigurationLifeSpan)));
        }

        protected internal virtual T GetConfiguration<T>(IConfigurationMetadata configurationMetadata = null)
            where T: IConfiguration
        {
            IConfigurationMetadata metadata = GetMetadataForConfig<T>(configurationMetadata);
            ICachedObject<T> cachedObject = CachingService.Retrieve<T>(metadata.ConfigurationName);
            T configuration;

            if (cachedObject == null || cachedObject.ExpireTime < DateTime.UtcNow || cachedObject.Value == null)
            {
                configuration = GetConfigurationFromSource<T>(metadata);
            }
            else
            {
                configuration = cachedObject.Value;
            }

            return configuration;
        }

        protected virtual T GetConfigurationFromSource<T>(IConfigurationMetadata configurationMetadata)
            where T: IConfiguration
        {
            T configuration = default(T);
            
            try
            {
                Stream configurationStream = configurationMetadata.UseStream
                    ? configurationMetadata.GetConfigurationStream()
                    : File.OpenRead(configurationMetadata.ConfigurationLocation);
                
                if (configurationStream == null)
                {
                    throw new InvalidOperationException("Unable to open configuration stream.");
                }
                    
                configuration = SerializationService.DeserializeObject<T>(configurationStream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to read or deserialize configuration using " +
                    (configurationMetadata.UseStream ? "Stream" : "Filesystem") + ". See Inner Exception for details.", ex);
            }
            configuration.ConfigurationMetadata = configurationMetadata;
            CachingService.Cache(configurationMetadata.ConfigurationName, configuration, DateTime.UtcNow, DateTime.UtcNow.Add(ConfigurationLifeSpan));

            return configuration;
        }

        protected virtual IConfigurationMetadata GetMetadataForConfig<T>(IConfigurationMetadata configurationMetadata)
            where T: IConfiguration
        {
            IConfigurationMetadata metadata = configurationMetadata;
            if (metadata == null)
            {
                T initialInstance = (T)Activator.CreateInstance(typeof(T));
                metadata = GetMetadataForConfigByReflection<T>();
            }
            return metadata;
        }

        protected virtual IConfigurationMetadata GetMetadataForConfigByReflection<T>()
            where T: IConfiguration
        {
            Type configurationMetaDataType = typeof(T).GetProperty("ConfigurationMetadata").PropertyType;
            IConfigurationMetadata metadata;
            try
            {
                metadata = (IConfigurationMetadata)Activator.CreateInstance(configurationMetaDataType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to create IConfigurationMetadata, and no metadata was passed in.", ex);
            }
            return metadata;
        }
    }
}