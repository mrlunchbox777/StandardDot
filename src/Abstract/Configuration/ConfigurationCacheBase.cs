using System;
using System.IO;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.CoreServices;

namespace StandardDot.Abstract.Configuration
{
    /// <summary>
    /// A base for configuration caching
    /// </summary>
    public abstract class ConfigurationCacheBase : IConfigurationCache
    {
        /// <param name="cachingService">The backing caching service to use</param>
        /// <param name="serializationService">A serialization service to use for reading configurations</param>
        /// <param name="configurationLifeSpan">How long cached configurations should be valid for</param>
        public ConfigurationCacheBase(ICachingService cachingService, ISerializationService serializationService,
            TimeSpan? configurationLifeSpan = null)
        {
            CachingService = cachingService;
            SerializationService = serializationService;
            ConfigurationLifeSpan = configurationLifeSpan;
        }

        protected virtual ICachingService CachingService { get; }

        protected virtual ISerializationService SerializationService { get; }

        protected virtual TimeSpan? ConfigurationLifeSpan { get; }

        public virtual int NumberOfConfigurations => CachingService.Count;

        /// <summary>
        /// Removes all configurations from cache
        /// </summary>
        public virtual void ResetCache()
        {
            CachingService.Clear();
        }

        /// <summary>
        /// Removes a configuration from cache
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
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

        /// <summary>
        /// Checks if a configuration exists. Will cache it if found.
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        /// <returns>If the configuration exists</returns>
        protected internal virtual bool DoesConfigurationExist<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            Tm metadata = GetMetadataForConfiguration<T, Tm>(configurationMetadata);
            return CachingService.ContainsKey(metadata.ConfigurationName);
        }


        /// <summary>
        /// Adds the configuration to the defined cache
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configuration">The configuration to add to cache</param>
        protected internal virtual void AddConfiguration<T, Tm>(T configuration)
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            DateTime cacheTime = DateTime.UtcNow;
            ClearConfiguration<T, Tm>(configuration.ConfigurationMetadata);
            CachingService.Cache(configuration.ConfigurationMetadata.ConfigurationName, configuration, cacheTime,
                (ConfigurationLifeSpan == null ? (DateTime?)null : cacheTime.Add(ConfigurationLifeSpan.Value)));
        }

        /// <summary>
        /// Gets the config from the cache if it can, from the source otherwise.
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        /// <returns>The configuration</returns>
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

        /// <summary>
        /// Gets the config from the source. Disposes of the stream if used. Caches the configuration if found.
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        /// <returns>The configuration</returns>
        protected virtual T GetConfigurationFromSource<T, Tm>(Tm configurationMetadata)
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new()
        {
            T configuration = default(T);
            
            try
            {
                using(Stream configurationStream = configurationMetadata.UseStream
                    ? configurationMetadata.GetConfigurationStream()
                    : File.OpenRead(configurationMetadata.ConfigurationLocation))
                {
                    if (configurationStream == null)
                    {
                        throw new InvalidOperationException("Unable to open configuration stream.");
                    }
                        
                    configuration = SerializationService.DeserializeObject<T>(configurationStream);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to read or deserialize configuration using " +
                    (configurationMetadata.UseStream ? "Stream" : "Filesystem") + ". See Inner Exception for details.", ex);
            }
            
            configuration.ConfigurationMetadata = configurationMetadata;
            AddConfiguration<T, Tm>(configuration);

            return configuration;
        }

        /// <summary>
        /// Gets the metadata for a configuration type
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        /// <returns>The configuration metadata</returns>
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