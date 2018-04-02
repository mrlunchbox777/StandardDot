using System;
using shoellibraries.Enums;

namespace shoellibraries.Abstract.Configuration
{
    public interface IConfigurationService
    {
        IConfigurationCache Cache { get; }

        void ResetConfigurations();

        void ClearConfiguration<T>(IConfigurationMetadata configurationMetadata = null) where T: IConfiguration;

        T GetConfiguration<T>(IConfigurationMetadata configurationMetadata = null) where T: IConfiguration;

        bool DoesConfigurationExist<T>(IConfigurationMetadata configurationMetadata = null) where T: IConfiguration;
    }
}