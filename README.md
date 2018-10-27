# README

![StandardDot](https://img.shields.io/badge/Standard%20Dot-0.0.1-blue.svg)
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-green.svg)
![.NET Core](https://img.shields.io/badge/.NET%20Core-2.0-green.svg)

## Standard Dot

[![Quality Gate Status](http://ec2-52-41-240-180.us-west-2.compute.amazonaws.com/api/project_badges/measure?project=StandardDot%3Abranch&metric=alert_status)](http://ec2-52-41-240-180.us-west-2.compute.amazonaws.com/dashboard?id=StandardDot%3Abranch) [![Build Status](http://ec2-34-220-188-250.us-west-2.compute.amazonaws.com:8080/buildStatus/icon?job=Standard Dot)](http://ec2-34-220-188-250.us-west-2.compute.amazonaws.com:8080/job/Standard Dot)

Basic .NET Standard Libraries

### What this repository is for

This repository is for basic code that can be imported into any .NET Standard compatible project.

* [Learn Markdown](https://bitbucket.org/tutorials/markdowndemo)
* [Markdown Cheatsheet](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet#code)

### Licenses

[MIT License](/LICENSE.txt)

### Other READMEs

* [Basics](/README-Basics.md)
* [Hmac Authentication](/src/Authentication/Hmac/API%20Integration%20Help/HMAC%20Authentication%20Help.md)

### How to get set up

#### Summary of set up

* Read through the whole readme here, and read the other readme's as needed
* Check the [Basics](/README-Basics.md) for setup if you are new

#### Configuration

* You need the [.NET Core SDK](https://www.microsoft.com/net/download/windows)

#### How to run tests

* The tests are included in the project and use [XUnit](https://xunit.github.io/). You can run them from [VS Code](https://code.visualstudio.com/) and a [Dot Net Core Extension](https://github.com/matijarmk/dotnet-core-commands) might help.

#### Deployment instructions

* There currently isn't a nuget package, but this project can be imported as a [subtree](https://medium.com/@v/git-subtrees-a-tutorial-6ff568381844)
* It is a current goal to get this working with cake and get CD set up with Jenkins and Sonar Qube

### Contribution guidelines

* Writing tests
	* We use [XUnit](https://xunit.github.io/)
	* We are aiming for >90% code coverage, so basically all code should have acompanying test
* Code review
	* All pull requests will need to be reviewed by an admin
* You can find results of testing and code quality analysis on [SonarQube](http://ec2-52-41-240-180.us-west-2.compute.amazonaws.com/dashboard?id=StandardDot%3Abranch)
* To get code to [`develop`](https://github.com/mrlunchbox777/shoellibraries/tree/develop) or [`master`](https://github.com/mrlunchbox777/shoellibraries/tree/master) there must a pull request (Which can only be accepted by an admin)