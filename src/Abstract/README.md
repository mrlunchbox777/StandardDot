# Abstract

## Navigation

* [Home](/README.md)
  * [Index](/docs/Index.md)

### Children

* [Caching](/src/Abstract/Caching/README.md)
* [Configuration](/src/Abstract/Configuration/README.md)
* [Core Services](/src/Abstract/CoreServices/README.md)
* [Data Structures](/src/Abstract/DataStructures/README.md)
* [Abstract Integration Tests](/src/AbstractIntegrationTests/README.md)
* [Abstract Unit Tests](/src/AbstractUnitTests/README.md)

## Info

This package contains a number of abstract classes that are used and implemented by other packages. It provides a layer of abstraction that gives the user freedom to implement and inject implementations as desired. For example there are several different caching services, and users can write their own caching service, as long as they implement [ICachingService](/src/Abstract/Caching/ICachingService.cs), then it can be substituted in anywhere ICachingService is used.

If a package needs a service/provider/factory/etc it should be using an interface or other abstraction from this package to get it so that users can select any implementation of that abstraction to use. As much as possible those packages should allow nulls for those services/providers/factories/etc so that if users do not want to use a particular part of a service they don't have to.

### Table of Contents

* [IApiKeyService](/src/Abstract/IApiKeyService.cs)
  * This is an interface that requires simple extension of `IDictionary<string, string>` to provide a way to get/match against `<appId, apiKey>`.
  * A simple way to implement it is a class that extends `Dictionary<string, string>` and implements `IApiKeyService`. It is not appropriate for all use cases and is often too simple, specifically for security driven applications. However, for testing purposes it will work, and is easy to replace with something more elegant, secure, and appropriate when moving to production.
  * Most of the caching services can be used to provide this kind of service.
