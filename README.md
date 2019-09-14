# README - Standard Dot

![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-green.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mrlunchbox777_StandardDot&metric=alert_status)](https://sonarcloud.io/dashboard?id=mrlunchbox777_StandardDot)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrlunchbox777_StandardDot&metric=coverage)](https://sonarcloud.io/dashboard?id=mrlunchbox777_StandardDot)

## What this repository is for

Basic .NET Standard Libraries

This repository is for basic code that can be imported into any .NET Standard compatible project.

### Licenses

[MIT License](/LICENSE)

## Usage

### [Documentation](/docs/Index.md)

There is a significant amount of inline documentation. In addition these readme's should give a general overview of each subsection.

### [Nuget](https://www.nuget.org/packages?q=standarddot)

All packages generated here will be deployed to nuget and can be implemented by add them to your project.

### [Report Issues and Request Features](https://github.com/mrlunchbox777/StandardDot/issues/new)

If you have any features you'd like to request, problems you have found, or questions **[open an issue](https://github.com/mrlunchbox777/StandardDot/issues/new)**

### Other READMEs

* [Stack Basics](/docs/README-Basics.md)
* [Learn Markdown](https://bitbucket.org/tutorials/markdowndemo)
* [Markdown Cheatsheet](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet#code)

## How to get set up

### Summary of set up

* Read through the whole readme here, and read the other readme's as needed
* Check the [Stack Basics](docs/README-Basics.md) for setup if you are new

### Configuration

* You need the [.NET Core SDK](https://www.microsoft.com/net/download/windows)

### How to run tests

* The tests are included in the project and use [XUnit](https://xunit.github.io/). You can run them from [VS Code](https://code.visualstudio.com/) and a [Dot Net Core Extension](https://github.com/matijarmk/dotnet-core-commands) might help.

### Deployment instructions

* You can find all of the StandardDot packages on [nuget.org](https://www.nuget.org/packages?q=StandardDot)
* The goal is to have a readme for each project that will give general instructions for usage on each project
* This project is built and tested automatically by [Jenkins](http://ec2-34-217-32-249.us-west-2.compute.amazonaws.com:8080/) and [SonarQube](http://ec2-52-41-240-180.us-west-2.compute.amazonaws.com/dashboard?id=StandardDot%3Adevelop)

## Contribution guidelines

* Writing tests
	* We use [XUnit](https://xunit.github.io/)
	* We are aiming for >90% code coverage, so basically all code should have acompanying test
* Code review
	* All pull requests will need to be reviewed by an admin
* You can find results of testing and code quality analysis on [SonarQube](http://ec2-52-41-240-180.us-west-2.compute.amazonaws.com/dashboard?id=StandardDot%3Abranch)
* To get code to [`develop`](https://github.com/mrlunchbox777/shoellibraries/tree/develop) or [`master`](https://github.com/mrlunchbox777/shoellibraries/tree/master) there must a pull request (Which can only be accepted by an admin)