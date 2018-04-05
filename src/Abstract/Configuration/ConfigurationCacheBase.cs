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

        protected internal virtual void ClearConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            Tm metadata = GetMetadataForConfiguration<T, Tm>(configurationMetadata);
            if (CachingService.ContainsKey(metadata.ConfigurationName))
            {
                CachingService.Remove(metadata.ConfigurationName);
            }
        }

        protected internal virtual bool DoesConfigurationExist<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            Tm metadata = GetMetadataForConfiguration<T, Tm>(configurationMetadata);
            return CachingService.ContainsKey(metadata.ConfigurationName);
        }

        protected internal virtual void AddConfiguration<T, Tm>(T configuration)
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            DateTime cacheTime = DateTime.UtcNow;
            ClearConfiguration<T, Tm>(configuration.ConfigurationMetadata);
            CachingService.Cache(configuration.ConfigurationMetadata.ConfigurationName, configuration, cacheTime, (cacheTime.Add(ConfigurationLifeSpan)));
        }

        protected internal virtual T GetConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            Tm metadata = GetMetadataForConfiguration<T, Tm>(configurationMetadata);
            ICachedObject<T> cachedObject = CachingService.Retrieve<T>(metadata.ConfigurationName);
            T configuration;

            if (cachedObject == null || cachedObject.ExpireTime < DateTime.UtcNow || cachedObject.Value == null)
            {
                configuration = GetConfigurationFromSource<T, Tm>(metadata);
            }
            else
            {
                configuration = cachedObject.Value;
            }

            return configuration;
        }

        // disposes of the stream if used
        protected virtual T GetConfigurationFromSource<T, Tm>(Tm configurationMetadata)
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            T configuration = default(T);
            Stream configurationStream = null;
            
            try
            {
                configurationStream = configurationMetadata.UseStream
                    ? configurationMetadata.GetConfigurationStream()
                    : File.OpenRead(configurationMetadata.ConfigurationLocation);
                
                if (configurationStream == null)
                {
                    throw new InvalidOperationException("Unable to open configuration stream.");
                }
                    
                configuration = SerializationService.DeserializeObject<T>(configurationStream);
                configurationStream.Dispose();
            }
            catch (Exception ex)
            {
                configurationStream?.Dispose();
                throw new InvalidOperationException("Unable to read or deserialize configuration using " +
                    (configurationMetadata.UseStream ? "Stream" : "Filesystem") + ". See Inner Exception for details.", ex);
            }
            
            configuration.ConfigurationMetadata = configurationMetadata;
            CachingService.Cache(configurationMetadata.ConfigurationName, configuration, DateTime.UtcNow, DateTime.UtcNow.Add(ConfigurationLifeSpan));

            return configuration;
        }

        /// <summary>
        /// Gets the metadata for a configuration type
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        protected virtual Tm GetMetadataForConfiguration<T, Tm>(Tm configurationMetadata)
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            Tm metadata = configurationMetadata;
            if (metadata == null)
            {
                metadata = new Tm();
            }
            return metadata;
        }
    }
}