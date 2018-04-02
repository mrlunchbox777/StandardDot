using System;
using System.IO;
using shoellibraries.Enums;

namespace shoellibraries.Abstract.Configuration
{
    public interface IConfigurationMetadata
    {
        string ConfigurationName { get; }
        
        string ConfigurationLocation { get; }

        bool UseStream { get; }

        Func<Stream> GetConfigurationStream { get; }
    }
}