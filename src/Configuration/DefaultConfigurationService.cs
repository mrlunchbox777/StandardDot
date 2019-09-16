using System;
using StandardDot.Abstract.Configuration;

namespace StandardDot.Configuration
{
	/// <summary>
	/// A Basic Configuration Service
	/// </summary>
	public class DefaultConfigurationService : ConfigurationServiceBase
	{
		/// <param name="configurationMetadata">The backing configuration cache</param>
		public DefaultConfigurationService(ConfigurationCacheBase cachingService)
			: base(cachingService)
		{
		}
	}
}
