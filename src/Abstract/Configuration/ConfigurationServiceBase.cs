using System;
using StandardDot.Abstract.Caching;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
	/// <summary>
	/// A base for a configuration service
	/// </summary>
	public abstract class ConfigurationServiceBase : IConfigurationService
	{
		/// <param name="configurationMetadata">The backing configuration cache</param>
		public ConfigurationServiceBase(ConfigurationCacheBase cachingService)
		{
			CacheBase = cachingService;
		}

		public virtual IConfigurationCache Cache => CacheBase;

		protected virtual ConfigurationCacheBase CacheBase { get; set; }

		/// <summary>
		/// Clears all cached configurations
		/// </summary>
		public virtual void ResetCachedConfigurations()
		{
			CacheBase.ResetCache();
		}

		/// <summary>
		/// Clears a specific configuration
		/// </summary>
		/// <typeparam name="T">The configuration type</typeparam>
		/// <typeparam name="Tm">The configuration metadata type</typeparam>
		/// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
		public virtual void ClearConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
			where T : IConfiguration<T, Tm>, new()
			where Tm : IConfigurationMetadata<T, Tm>, new()
		{
			CacheBase.ClearConfiguration<T, Tm>(configurationMetadata);
		}

		/// <summary>
		/// Gets a configuration, from cache if possible. Updates the cache if it has to do a hard pull
		/// </summary>
		/// <typeparam name="T">The configuration type</typeparam>
		/// <typeparam name="Tm">The configuration metadata type</typeparam>
		/// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
		/// <returns>The configuration</returns>
		public virtual T GetConfiguration<T, Tm>(Tm configurationMetadata = default(Tm))
			where T : IConfiguration<T, Tm>, new()
			where Tm : IConfigurationMetadata<T, Tm>, new()
		{
			return CacheBase.GetConfiguration<T, Tm>(configurationMetadata);
		}

		/// <summary>
		/// Checks if a configuration exists. Will cache it if found.
		/// </summary>
		/// <typeparam name="T">The configuration type</typeparam>
		/// <typeparam name="Tm">The configuration metadata type</typeparam>
		/// <param name="configurationMetadata">The metadata related to the configuration, default gathered from Type Data</param>
		/// <returns>If the configuration exists</returns>
		public virtual bool DoesConfigurationExist<T, Tm>(Tm configurationMetadata = default(Tm))
			where T : IConfiguration<T, Tm>, new()
			where Tm : IConfigurationMetadata<T, Tm>, new()
		{
			return CacheBase.DoesConfigurationExist<T, Tm>(configurationMetadata);
		}
	}
}
