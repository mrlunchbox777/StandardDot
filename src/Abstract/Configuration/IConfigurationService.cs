using System;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
    /// <summary>
    /// An interface for configuration management
    /// </summary>
    public interface IConfigurationService
    {
        IConfigurationCache Cache { get; }

        /// <summary>
        /// Resets the cached configurations
        /// </summary>
        void ResetCachedConfigurations();

        /// <summary>
        /// Clears specific configuration from cache
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        void ClearConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new();
        
        /// <summary>
        /// Gets a configuration, from cache if possible. Updates the cache if it has to do a hard pull
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        /// <returns>The configuration</returns>
        T GetConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new();
        
        /// <summary>
        /// Checks to see if a configuration exists
        /// </summary>
        /// <typeparam name="T">The configuration type</typeparam>
        /// <typeparam name="Tm">The configuration metadata type</typeparam>
        /// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
        /// <returns>If the service has the configuration</returns>
        bool DoesConfigurationExist<T, Tm>(Tm configurationMetadata = default(Tm))
            where T: IConfiguration<T, Tm>, new()
            where Tm: IConfigurationMetadata<T, Tm>, new();
    }
}