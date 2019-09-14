# Abstract Unit Tests

## Navigation

* [Home](/README.md)
	* [Index](/docs/Index.md)
	* [Abstract](/src/Abstract/README.md)

### Children

* [Abstract Implementations](/src/AbstractUnitTests/AbstractImplementations/README.md)
* [Abstract Objects](/src/AbstractUnitTests/AbstractObjects/README.md)
* [Caching Unit Tests](/src/AbstractUnitTests/Caching/README.md)
* [Configuration Unit Tests](/src/AbstractUnitTests/Configuration/README.md)
* [Core Services Unit Tests](/src/AbstractUnitTests/CoreServices/README.md)
* [Test Configuration Metadatas](/src/AbstractUnitTests/TestConfigurationMetadatas/README.md)
* [Test Configurations](/src/AbstractUnitTests/TestConfigurations/README.md)

## Duplicated Code

There are several folders and files that are directly copied from test classes (since it directly references abstract, it can't use testclasses that uses it via nuget)

* Abstract Implementations (folder)
* Test Configurations (folder)
* Configuration Metadatas (folder)
* Foobar (.cs)
* Check Dispose Stream (.cs)
* testConfigurationJson.json
* testConfigurationJson2.json

THE TEST CLASSES VERSION SHOULD BE THE SYSTEM OF RECORD.

ANY CHANGES SHOULD BE MADE THERE, THEN THAT FILE COPIED HERE