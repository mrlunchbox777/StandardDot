using System;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
    public interface IConfiguration
    {
        IConfigurationMetadata ConfigurationMetadata { get; set; }
    }
}