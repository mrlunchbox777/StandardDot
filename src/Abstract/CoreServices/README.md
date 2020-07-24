# Core Services

## Navigation

* [Home](/README.md)
  * [Index](/docs/Index.md)
    * [Abstract](/src/Abstract/README.md)

### Children

* [Core Services Integration Tests](/src/AbstractIntegrationTests/CoreServices/README.md)
* [Core Services Unit Tests](/src/AbstractUnitTests/CoreServices/README.md)

## Info

This namespace provides shared interfaces for basic services, e.g. logging, pagination, serialization, etc. Many of these are provided for by C# or very popular 3rd party services, where possible the most common of these will have implementations.

### Table of Contents

* [ILogBaseEnmerable](/src/Abstract/CoreServices/ILogBaseEnumerable.cs)
  * This is an interface that defines an IEnumerable of [LogBase](/src/Dto/CoreServices/LogBase.cs). It is used by several of the logging functions in StandardDot.
* [ILogEnmerable](/src/Abstract/CoreServices/ILogEnumerable.cs)
  * This is an interface that defines an IEnumerable of [Log](/src/Dto/CoreServices/Log.cs). It is used by several of the logging functions in StandardDot.
* [ILoggingService](/src/Abstract/CoreServices/ILoggingService.cs)
  * This is an interface that defines a service that allows recording of information which may or may not include [Logs](/src/Dto/CoreServices/Log.cs), Exceptions (System.Exception), and Objects (System.Object). 
  
  
* [test](/src/Abstract/CoreServices/test.cs)
  * This is the generic class that extends [test](/src/Abstract/CoreServices/test.cs). It provides a base object that defines a related metadata that must inherit from [test](/src/Abstract/Configuration/test.cs). It should be extended to define a configuration with some associated metadata.
