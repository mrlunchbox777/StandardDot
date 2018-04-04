using System;
using StandardDot.Abstract.Caching;
using StandardDot.Enums;

namespace StandardDot.Abstract.Configuration
{
    public interface IConfigurationCache
    {
        void ResetCache();

        int NumberOfConfigurations { get; }
    }
}