using System;
using StandardDot.Abstract.Caching;
using StandardDot.Abstract.Configuration;
using StandardDot.Abstract.CoreServices;

namespace StandardDot.Configuration
{
	/// <summary>
	/// A Basic Configuration Cache
	/// </summary>
	public class DefaultConfigurationCache : ConfigurationCacheBase
	{
		/// <param name="cachingService">The backing caching service to use</param>
		/// <param name="serializationService">A serialization service to use for reading configurations</param>
		/// <param name="configurationLifeSpan">How long cached configurations should be valid for</param>
		public DefaultConfigurationCache(ICachingService cachingService, ISerializationService serializationService,
			TimeSpan configurationLifeSpan)
			: base(cachingService, serializationService, configurationLifeSpan)
		{
		}
	}
}
