using System;
using StandardDot.Abstract.Caching;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
	/// <summary>
	/// An interface for a configuration cache
	/// </summary>
	public interface IConfigurationCache
	{
		/// <summary>
		/// Resets the cached configurations
		/// </summary>
		void ResetCache();

		int NumberOfConfigurations { get; }
	}
}
