using System;
using System.IO;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
	/// <summary>
	/// An interface for basic configuration metadata
	/// </summary>
	/// <typeparam name="T">The configuration type</typeparam>
	/// <typeparam name="Tm">The configuration metadata type</typeparam>
	public interface IConfigurationMetadata<out T, out Tm>
		where T : IConfiguration<T, Tm>, new()
		where Tm : IConfigurationMetadata<T, Tm>, new()
	{
		Type ConfigurationType { get; }

		string ConfigurationName { get; }

		string ConfigurationLocation { get; }

		/// <summary>
		/// If the configuration is gathered from the stream instead of the configuration location
		/// </summary>
		bool UseStream { get; }

		/// <summary>
		/// A function that gets the configuration stream. Only called on hard pulls.
		/// </summary>
		Func<Stream> GetConfigurationStream { get; }
	}
}
