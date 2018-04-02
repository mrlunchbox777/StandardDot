using System;
using shoellibraries.Abstract.Caching;
using shoellibraries.Enums;

namespace shoellibraries.Abstract.Configuration
{
    public interface IConfigurationCache
    {
        void ResetCache();

        int NumberOfConfigurations { get; }
    }
}