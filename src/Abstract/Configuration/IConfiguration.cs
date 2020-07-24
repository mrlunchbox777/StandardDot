using System;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
	/// <summary>
	/// An interface for a configuration
	/// </summary>
	/// <typeparam name="T">The configuration type</typeparam>
	/// <typeparam name="Tm">The configuration metadata type</typeparam>
	public interface IConfiguration<T, Tm>
		where T : IConfiguration<T, Tm>, new()
		where Tm : IConfigurationMetadata<T, Tm>, new()
	{
		Tm ConfigurationMetadata { get; set; }
	}
}
