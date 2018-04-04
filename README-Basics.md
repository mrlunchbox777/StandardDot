# README Basics

[Home](/README.md)

## Basic Setup

### The Stack

* We use [Git](https://www.codecademy.com/courses/learn-git/lessons/git-workflow)
    * We use the [Gitflow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow) workflow
        * [`master`](https://github.com/mrlunchbox777/StandardDot/tree/master) is protected, only admins can accept merge requests
        * [`develop`](https://github.com/mrlunchbox777/StandardDot/tree/develop) is protected, only admins can accept merge requests
        * Our Gitflow Standards
            * Production branch - [`master`](https://github.com/mrlunchbox777/StandardDot/tree/master)
            * Development branch - [`develop`](https://github.com/mrlunchbox777/StandardDot/tree/develop)
            * Feature branch prefix - `feature/`
            * Release branch prefix - `release/`
            * Hotfix branch prefix - `hotfix/`
            * Version tag prefix - `v`
            * Support branch prefix - `support/`
* This is a [C#](https://www.tutorialspoint.com/csharp/index.htm) stack
    * Current using [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/whats-new/whats-new-in-dotnet-standard?tabs=csharp#whats-new-in-the-net-standard-20) and [.NET Core 2.0](https://docs.microsoft.com/en-us/dotnet/core/whats-new/)
    * [What's the difference?](https://msdn.microsoft.com/en-us/magazine/mt842506.aspx)

### Dependencies

* [VS Code](https://code.visualstudio.com/) is our preferred development environment
    * The [C# Integration](https://github.com/OmniSharp/omnisharp-vscode) is all but necessary
    * The [.NET Core SDK](https://github.com/matijarmk/dotnet-core-commands) can be integrated
    * [Solution Management](https://github.com/fernandoescolar/vscode-solution-explorer) and [Code Lensing](https://github.com/eamodio/vscode-gitlens) is also available
* [Git](https://git-scm.com/downloads) - Command Line
    * If you like GUIs there are options
        * [SourceTree](https://www.sourcetreeapp.com/)
        * [Git Kraken](https://www.gitkraken.com/)
        * [GitHub Desktop](https://desktop.github.com/)
    * You will probably want an external merge tool
        * [P4 Merge](https://www.perforce.com/downloads/visual-merge-tool)