using System;
using System.Runtime.Serialization;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
    /// <summary>
    /// An base class for a configuration
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <typeparam name="Tm">The configuration metadata type</typeparam>
    [DataContract]
    public abstract class ConfigurationBase<T, Tm> : IConfiguration<T, Tm>
        where T: ConfigurationBase<T, Tm>, new()
        where Tm: ConfigurationMetadataBase<T, Tm>, new()
    {
        public ConfigurationBase()
        { }

        /// <param name="configurationMetadata">The metadata for the configuration</param>
        public ConfigurationBase(Tm configurationMetadata)
        {
            ConfigurationMetadata = configurationMetadata;
        }

        /// <summary>
        /// This should never be serialized
        /// </summary>
        [IgnoreDataMember]
        public virtual Tm ConfigurationMetadata { get; set; }
    }
}