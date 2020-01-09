# Configuration

## Navigation

* [Home](/README.md)
  * [Index](/docs/Index.md)
    * [Abstract](/src/Abstract/README.md)

### Children

* [Configuration Integration Tests](/src/AbstractIntegrationTests/Configuration/README.md)
* [Configuration Unit Tests](/src/AbstractUnitTests/Configuration/README.md)

## Info

This namespace provides shared interfaces for configuration. These configuration files are a convenient alternative to those provided by C# by default that require a full deploy or restarting a service. They allow caching to provide speed, separation of duties, and inheritance.

### Table of Contents

* [ConfigurationBase](/src/Abstract/Configuration/ConfigurationBase.cs)
  * This is the generic class that extends [IConfigurationBase](/src/Abstract/Configuration/IConfigurationBase.cs). It provides a base object that defines a related metadata that must inherit from [ConfigurationMetadataBase](/src/Abstract/Configuration/ConfigurationMetadataBase.cs). It should be extended to define a configuration with some associated metadata.
* [ConfigurationCacheBase](/src/Abstract/Configuration/ConfigurationCacheBase.cs)
  * This is the generic class that extends [IConfigurationCache](/src/Abstract/Configuration/IConfigurationCache.cs). It provides a basic caching layer for configuration management. 
* [ConfigurationMetadataBase](/src/Abstract/Configuration/ConfigurationMetadataBase.cs)
  * This is the generic class that extends [IConfigurationMetadata](/src/Abstract/Configuration/IConfigurationMetadata.cs). It provides metadata mostly related to how to find and import the configuration data.
* [ConfigurationServiceBase](/src/Abstract/Configuration/ConfigurationServiceBase.cs)
  * This is the generic class that extends [IConfigurationService](/src/Abstract/Configuration/IConfigurationService.cs). It provides a basic service for managing the configuration lifecycle for any configuration that conforms to [IConfiguration](/src/Abstract/Configuration/IConfiguration.cs).
* [IConfiguration](/src/Abstract/Configuration/IConfiguration.cs)
  * This defines an interface that can be implemented to provide configuration data and requires a related metadata that must implement [IConfigurationMetadata](/src/Abstract/Configuration/IConfigurationMetadata.cs).
* [IConfigurationCache](/src/Abstract/Configuration/IConfigurationCache.cs)
  * This defines an interface that can be implemented to provide basic configuration caching.
* [IConfigurationMetadata](/src/Abstract/Configuration/IConfigurationMetadata.cs)
  * This defines an interface that can be implemented to provide metadata mostly related to how to find and import the configuration data.
* [IConfigurationService](/src/Abstract/Configuration/IConfigurationService.cs)
  * This defines an interface that can be implemented to provide a basic service for managing the configuration lifecycle for any configuration that conforms to [IConfiguration](/src/Abstract/Configuration/IConfiguration.cs).
