using System;
using System.IO;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
    public interface IConfigurationMetadata
    {
        string ConfigurationName { get; }
        
        string ConfigurationLocation { get; }

        bool UseStream { get; }

        Func<Stream> GetConfigurationStream { get; }
    }
}