# Caching

## Navigation

* [Home](/README.md)
	* [Index](/docs/Index.md)
		* [Abstract](/src/Abstract/README.md)

### Children

* [Caching Integration Tests](/src/AbstractIntegrationTests/Caching/README.md)
* [Caching Unit Tests](/src/AbstractUnitTests/Caching/README.md)

## Info

This namespace provides shared interfaces for caching services. Any StandardDot package that requires a caching service should require these rather than any specific implementation to allow for ease of interchangibility of services (via IOC or any other method).

### Table of Contents

* [ICachedObject](/src/Abstract/Caching/ICachedObject.cs)
	* This is the generic interface that extends [ICachedObjectBasic](/src/Abstract/Caching/ICachedObjectBasic.cs). It is used most commonly for storage and retrival to allow for proper serialization and storage of the target object(s).
* [ICachedObjectBasic](/src/Abstract/Caching/ICachedObjectBasic.cs)
	* This is the interface that defines the majority of metadata and data used by [ICachingService](/src/Abstract/Caching/ICachingService.cs). This includes cached time, expire time, and value. It does not use generics to allow for non generic calls and implementations.
* [ICachingService](/src/Abstract/Caching/ICachingService.cs)
	* This is the interface that defines a caching service, and defines it as an extension of `ILazyDictionary<string, ICachedObjectBasic>`. As a dictionary it uses the [ICachedObjectBasic](/src/Abstract/Caching/ICachedObjectBasic.cs) as all values in a dictionary must be of the same type, but not all values in the cache are guarunteed to be of the same type.
		* Being able to rewrite this to use some reflection to ensure generics and/or strong typing would be preferable provided it doesn't impact performance significantly.