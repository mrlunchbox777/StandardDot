using System;
using System.IO;

namespace StandardDot.Abstract.Configuration
{
    /// <summary>
    /// An interface for basic configuration metadata
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <typeparam name="Tm">The configuration metadata type</typeparam>
    public abstract class ConfigurationMetadataBase<T, Tm> : IConfigurationMetadata<T, Tm>
        where T: ConfigurationBase<T, Tm>, new()
        where Tm: ConfigurationMetadataBase<T, Tm>, new()
    {
        public ConfigurationMetadataBase()
        { }

        /// <param name="configurationLocation">The location for the configuration</param>
        public ConfigurationMetadataBase(string configurationLocation)
        {
            ConfigurationLocation = configurationLocation;
            GetConfigurationStream = null;
            UseStream = false;
        }

        /// <param name="getConfigurationStream">
        /// A Func that gets a Stream that contains a string representation of the configuration
        /// </param>
        public ConfigurationMetadataBase(Func<Stream> getConfigurationStream)
        {
            GetConfigurationStream = getConfigurationStream;
            ConfigurationLocation = null;
            UseStream = true;
        }

        public virtual Type ConfigurationType { get => typeof(T); }

        public abstract string ConfigurationName { get; }
        
        public virtual string ConfigurationLocation { get; }

        /// <summary>
        /// If the configuration is gathered from the stream instead of the configuration location
        /// </summary>
        public virtual bool UseStream { get; }

        /// <summary>
        /// A function that gets the configuration stream. Only called on hard pulls.
        /// </summary>
        public virtual Func<Stream> GetConfigurationStream { get; }
    }
}