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
  * This is the generic interface that extends [IConfigurationBase](/src/Abstract/Configuration/IConfigurationBase.cs). It provides a base object that defines a related metadata that must inherit from [ConfigurationMetaDataBase](/src/Abstract/Configuration/ConfigurationMetaDataBase.cs).
* [ConfigurationCacheBase](/src/Abstract/Configuration/ConfigurationCacheBase.cs)
  * This is the generic interface that extends [IConfigurationCache](/src/Abstract/Configuration/IConfigurationCache.cs)
