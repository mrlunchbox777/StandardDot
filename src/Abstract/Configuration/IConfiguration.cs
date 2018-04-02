using System;
using shoellibraries.Enums;

namespace shoellibraries.Abstract.Configuration
{
    public interface IConfiguration
    {
        IConfigurationMetadata ConfigurationMetadata { get; }
    }
}