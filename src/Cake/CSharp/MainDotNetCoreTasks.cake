#addin "nuget:?package=NuGet.Core"

// Load other scripts.
#load "MainTasks.cake"

//////////////////////////////////////////////////////////////
// DotNet Core Tasks
//////////////////////////////////////////////////////////////

Task("Build-Project-DotNet-Core-Alias")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Build Project.");
    }
    if (Config.MSBuildInfo.ShouldFlatten())
    {
        // StartProcess("dotnet");
        DotNetCoreBuild(Config.ProjectInfo.ProjectFile.ToString(), new DotNetCoreBuildSettings
            {
                //.WithTarget(Config.ProjectInfo.ProjectName) //.Replace('.','_')
                Configuration = "Release",
                // .WithProperty("Platform", Config.MSBuildInfo.Platform)        
                // .WithProperty("VisualStudioVersion", Config.MSBuildInfo.MsBuildVersion)
                // .WithProperty("PipelineDependsOnBuild", "false")
                OutputDirectory = Config.ProjectInfo.FlattenOutputDirectory
                // .UseToolVersion(MSBuildToolVersion.Default)
                // .SetVerbosity(Verbosity.Minimal)
                // .SetMaxCpuCount(1)
            });
    }
    else
    {
        DotNetCoreBuild(Config.ProjectInfo.ProjectFile.ToString(), new DotNetCoreBuildSettings
            {
                //.WithTarget(Config.ProjectInfo.ProjectName) //.Replace('.','_')
                Configuration = Config.MSBuildInfo.MsBuildConfig(),
                // .WithProperty("Platform", Config.MSBuildInfo.Platform)        
                // .WithProperty("VisualStudioVersion", Config.MSBuildInfo.MsBuildVersion)
                // .UseToolVersion(MSBuildToolVersion.Default)
                // .SetVerbosity(Verbosity.Minimal)
                // .SetMaxCpuCount(1)
            });
        // MSBuild(Config.ProjectInfo.ProjectFile, new MSBuildSettings()
        //     //.WithTarget(Config.ProjectInfo.ProjectName) //.Replace('.','_')
        //     .SetConfiguration(Config.MSBuildInfo.MsBuildConfig())
        //     .WithProperty("Platform", Config.MSBuildInfo.Platform)        
        //     .WithProperty("VisualStudioVersion", Config.MSBuildInfo.MsBuildVersion)
        //     .UseToolVersion(MSBuildToolVersion.Default)
        //     .SetVerbosity(Verbosity.Minimal)
        //     .SetMaxCpuCount(1));
    }
    
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Check for c# syntax/runtime errors",
            "Try local compilation",
            "Ensure the .NET version and packages can be compiled with cake"
        },
        true
        );
});

Task("Build-Project-DotNet-Core")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Build Project.");
    }
    StartProcess("dotnet",
        " build "
        + "\"" + Config.ProjectInfo.ProjectFile.ToString() + "\""
        + " -c " + Config.MSBuildInfo.MsBuildConfig()
        + (string.IsNullOrWhiteSpace(Config.MSBuildInfo.TargetFramework) ? "" : " -f " + Config.MSBuildInfo.TargetFramework)
        + (Config.Nuget.Force ? " --force" : "")
        + (Config.MSBuildInfo.NoDependencies ? " --no-dependencies" : "")
        + (Config.MSBuildInfo.NoIncremental ? " --no-incremental" : "")
        + (Config.Nuget.NoRestore ? " --no-restore" : "")
        + (Config.MSBuildInfo.ShouldFlatten() ? " -o \"" + Config.ProjectInfo.FlattenOutputDirectory + "\"" : "")
        + (string.IsNullOrWhiteSpace(Config.Nuget.RunTimeVersion) ? "" : " --runtime " + Config.Nuget.RunTimeVersion)
        + (string.IsNullOrWhiteSpace(Config.Nuget.VerbosityLevel) ? "" : " -v " + Config.Nuget.VerbosityLevel)
        + (string.IsNullOrWhiteSpace(Config.Nuget.VersionSuffix) ? "" : " --version-suffix \"" + Config.Nuget.VersionSuffix + "\"")
    );
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Check for c# syntax/runtime errors",
            "Try local compilation",
            "Ensure the .NET version and packages can be compiled with cake"
        },
        true
        );
});

//////////////////////////////////////////////////////////////
// Unit Test Tasks
//////////////////////////////////////////////////////////////

Task("DotNet-Core-Run-Unit-Test")
    .WithCriteria(() => DirectoryExists(Config.UnitTests.UnitTestDirectoryPath))
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Unit Tests.");
    }

    Config.MSBuildInfo.IsRunningTests = true;
    try
    {
        StartProcess("dotnet",
            " test "
            + "\"" + Config.UnitTests.ProjectFile.ToString() + "\""
            + (string.IsNullOrWhiteSpace(Config.UnitTests.TestAdapterPath) ? "" : " -a " + Config.UnitTests.TestAdapterPath)
            + (Config.UnitTests.TestBlame ? " --blame" : "")
            + " -c " + Config.MSBuildInfo.MsBuildConfig()
            + (string.IsNullOrWhiteSpace(Config.UnitTests.DataCollectorName) ? "" : " -d " + Config.UnitTests.DataCollectorName)
            + (string.IsNullOrWhiteSpace(Config.UnitTests.TargetFramework) ? "" : " -f " + Config.UnitTests.TargetFramework)
            + (string.IsNullOrWhiteSpace(Config.UnitTests.FilterExpression) ? "" : " --filter " + Config.UnitTests.FilterExpression)
            + (Config.UnitTests.NoBuildForTest ? " --no-build" : "")
            + (Config.UnitTests.NoRestoreForTest ? " --no-restore" : "")
            + (Config.MSBuildInfo.ShouldFlatten() ? " -o \"" + Config.ProjectInfo.FlattenOutputDirectory + "\"" : "")
            + " -r \"" + (string.IsNullOrWhiteSpace(Config.UnitTests.ResultsDirectory) ? Config.ProjectInfo.ProjectDirectory : Config.UnitTests.ResultsDirectory) + "\""
            + (string.IsNullOrWhiteSpace(Config.UnitTests.SettingsFile) ? "" : " -s " + Config.UnitTests.SettingsFile)
            + (Config.UnitTests.ListTests ? " -t" : "")
            + (string.IsNullOrWhiteSpace(Config.Nuget.VerbosityLevel) ? "" : " -v " + Config.Nuget.VerbosityLevel)
        );
    }
    catch (Exception)
    {
        Config.MSBuildInfo.IsRunningTests = false;
        throw;
    }
    Config.MSBuildInfo.IsRunningTests = false;
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Check for xunit syntax/runtime errors",
            "ENSURE THE UNIT TESTS HAVE AT LEAST 1 XUNIT TEST",
            "Check for file locks"
        },
        true
        );
});

//////////////////////////////////////////////////////////////
// Deploy Nuget
//////////////////////////////////////////////////////////////

Task("DotNet-Core-Pack-Nuget-Package")
    .WithCriteria(() => Config.Nuget.CreateNugetPackage)
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Pack Nuget Package.");
    }

    StartProcess("dotnet",
        " pack "
        + "\"" + Config.ProjectInfo.ProjectFile.ToString() + "\""
        + (Config.Nuget.Force ? " --force" : "")
        + (Config.Nuget.IncludeSource ? " --include-source" : "")
        + (Config.Nuget.Symbols ? " --include-symbols" : "")
        + (Config.Nuget.BuildForPack ? "" : " --no-build")
        + (Config.Nuget.IgnoreDependencies ? " --no-dependencies" : "")
        + (Config.Nuget.NoRestore ? " --no-restore" : "")
        + " -o \"" + (string.IsNullOrWhiteSpace(Config.Nuget.PackDirectory) ? Config.ProjectInfo.ProjectDirectory : Config.Nuget.PackDirectory) + "\""
        + (string.IsNullOrWhiteSpace(Config.Nuget.RunTimeVersion) ? "" : " --runtime " + Config.Nuget.RunTimeVersion)
        + (Config.Nuget.Servicable ? " -s" : "")
        + (string.IsNullOrWhiteSpace(Config.Nuget.VersionSuffix) ? "" : " --version-suffix \"" + Config.Nuget.VersionSuffix + "\"")
        + (string.IsNullOrWhiteSpace(Config.Nuget.VerbosityLevel) ? "" : " -v " + Config.Nuget.VerbosityLevel)
    );
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure dotnet pack is possible",
            "Ensure the nuget server is up",
            "Ensure nuget got installed"
        },
        true
        );
});

Task("DotNet-Core-Deploy-Nuget-Package")
    .WithCriteria(() => Config.Nuget.CreateNugetPackage)
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Deploy Nuget Package.");
    }

    var packageFinder = (string.IsNullOrWhiteSpace(Config.Nuget.PackDirectory)
            ? Config.ProjectInfo.ProjectDirectory
            : Config.Nuget.PackDirectory)
        + "/*.nupkg";

    var package = GetFiles(packageFinder).FirstOrDefault();

    if (package == null)
    {
        throw new InvalidOperationException("Unable to find .nupkg. In - " + packageFinder);
    }

    if (string.IsNullOrWhiteSpace(Config.Nuget.ApiKey))
    {
        throw new InvalidOperationException("Unable to find ApiKey. Please ensure 'NUGET_APIKEY' is set as an environmental variable.");
    }

    if (string.IsNullOrWhiteSpace(Config.Nuget.Server))
    {
        throw new InvalidOperationException("Unable to find Nuget Server. Please ensure Config.Nuget.Server is set during setup.");
    }

    StartProcess("dotnet",
        " nuget push "
        + "\"" + package.ToString() + "\""
        + (Config.Nuget.DisableBuffering ? " -d" : "")
        + (Config.Nuget.ForceEnglishOutput ? " --force-english-output" : "")
        + " -k " + Config.Nuget.ApiKey
        + (Config.Nuget.NoPushSymbols ? " -n" : "")
        + (Config.Nuget.NoServiceEndpoint ? " --no-service-endpoint" : "")
        + " -s " + Config.Nuget.Server
        + (string.IsNullOrWhiteSpace(Config.Nuget.SymbolApiKey) ? "" : " -sk " + Config.Nuget.SymbolApiKey)
        + (string.IsNullOrWhiteSpace(Config.Nuget.SymbolSource) ? "" : " -ss " + Config.Nuget.SymbolSource)
        + (Config.Nuget.Timeout > 0 ? " -t " + Config.Nuget.Timeout : "")
    );
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure nuspec exists",
            "Ensure the nuget server is up",
            "Ensure nuget got installed",
            "Ensure NUGET_APIKEY is an environmental variable"
        },
        true
        );
});