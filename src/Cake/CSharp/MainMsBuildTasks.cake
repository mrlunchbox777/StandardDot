#addin "Cake.IIS"
#addin "nuget:?package=System.ServiceProcess.ServiceController"

// Load other scripts.
#load "MainTasks.cake"


//////////////////////////////////////////////////////////////
// MSBuild Tasks
//////////////////////////////////////////////////////////////

Task("Restore-CSharp-Nuget-Packages")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Restore Packages.");
    }
    var maxRetryCount = 5;
    var toolTimeout = 1d;
    Policy
        .Handle<Exception>()
        .Retry(maxRetryCount, (exception, retryCount, context) => {
            if (retryCount == maxRetryCount)
            {
                throw exception;
            }
            else
            {
                Verbose("{0}", exception);
                toolTimeout+=0.5;
            }})
        .Execute(()=> {
            NuGetRestore(Config.ProjectInfo.ProjectSolution, new NuGetRestoreSettings {
                Source = (string.IsNullOrWhiteSpace(Config.Nuget.Server)
                    ? null
                    : new List<string> {
                    Config.Nuget.Server
                }),
                ToolTimeout = TimeSpan.FromMinutes(toolTimeout),
                PackagesDirectory = Config.Nuget.PackagesDirectory
            });
        });
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Check that packages are available",
            "Check that the nuget server is available",
            "Try local compilation after deleting the packages directory",
            "Ensure the .NET version and packages can be compiled with cake"
        },
        true
        );
});

Task("Build-Project-MSBuild")
    .IsDependentOn("Restore-CSharp-NuGet-Packages")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Build Project.");
    }
    if (Config.MSBuildInfo.ShouldFlatten())
    {
        MSBuild(Config.ProjectInfo.ProjectFile, new MSBuildSettings()
            //.WithTarget(Config.ProjectInfo.ProjectName) //.Replace('.','_')
            .SetConfiguration("Release")
            .WithProperty("Platform", Config.MSBuildInfo.Platform)        
            .WithProperty("VisualStudioVersion", Config.MSBuildInfo.MsBuildVersion)
            .WithProperty("PipelineDependsOnBuild", "false")
            .WithProperty("OutputPath", Config.ProjectInfo.FlattenOutputDirectory)
            .UseToolVersion(MSBuildToolVersion.Default)
            .SetVerbosity(Verbosity.Minimal)
            .SetMaxCpuCount(1));
    }
    else
    {
        MSBuild(Config.ProjectInfo.ProjectFile, new MSBuildSettings()
            //.WithTarget(Config.ProjectInfo.ProjectName) //.Replace('.','_')
            .SetConfiguration(Config.MSBuildInfo.MsBuildConfig())
            .WithProperty("Platform", Config.MSBuildInfo.Platform)        
            .WithProperty("VisualStudioVersion", Config.MSBuildInfo.MsBuildVersion)
            .UseToolVersion(MSBuildToolVersion.Default)
            .SetVerbosity(Verbosity.Minimal)
            .SetMaxCpuCount(1));
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

//////////////////////////////////////////////////////////////
// IIS Tasks
//////////////////////////////////////////////////////////////

Task("StopAnApplicationPool")
    .WithCriteria(() => DirectoryExists(Config.ConfigurableSettings.RestartIIS))
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Stopping IIS.");
    }

    if (Config.ConfigurableSettings.UseRemoteServer)
    {
        StopPool(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationPoolName);
        StopSite(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationSiteName);
    } else
    {
        StopPool(Config.ConfigurableSettings.ApplicationPoolName);
        StopSite(Config.ConfigurableSettings.ApplicationSiteName);
    }
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Did all the settings load?",
            "Did you set the pool and site name?",
            "Is IIS running?"
        },
        true
        );
});

Task("StartAnApplicationPool")
    .WithCriteria(() => DirectoryExists(Config.ConfigurableSettings.RestartIIS))
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting IIS.");
    }

    if (Config.ConfigurableSettings.UseRemoteServer)
    {
        StartPool(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationPoolName);
        StartSite(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationSiteName);
    } else
    {
        StartPool(Config.ConfigurableSettings.ApplicationPoolName);
        StartSite(Config.ConfigurableSettings.ApplicationSiteName);
    }
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Did all the settings load?",
            "Did you set the pool and site name?",
            "Is IIS running?"
        },
        true
        );
});

//////////////////////////////////////////////////////////////
// Unit Test Tasks (we aren't doing any of these right now)
//////////////////////////////////////////////////////////////

Task("Build-Unit-Tests")
    .WithCriteria(() => DirectoryExists(Config.UnitTests.UnitTestDirectoryPath))
    .IsDependentOn("Restore-CSharp-NuGet-Packages")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Build Unit Tests.");
    }
    MSBuild(Config.ProjectInfo.ProjectSolution, new MSBuildSettings()
        .WithTarget(Config.UnitTests.UnitTestProjectName.Replace('.','_'))
        .SetConfiguration(Config.MSBuildInfo.MsBuildConfig())
        .WithProperty("Platform", Config.MSBuildInfo.Platform)        
        .WithProperty("Configuration", Config.MSBuildInfo.MsBuildConfig())
        .WithProperty("VisualStudioVersion", Config.MSBuildInfo.MsBuildVersion)
        .UseToolVersion(MSBuildToolVersion.Default)
        .SetVerbosity(Verbosity.Minimal)
        .SetMaxCpuCount(1));
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

Task("Run-Unit-Tests")
    .WithCriteria(() => DirectoryExists(Config.UnitTests.UnitTestDirectoryPath))
    .IsDependentOn("Build-Unit-Tests")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Run Unit Tests.");
    }
    string targetDir = Config.UnitTests.UnitTestDirectoryPath.FullPath + "/bin/" + Config.MSBuildInfo.MsBuildConfig();
    IEnumerable<FilePath> targetDLLs = new List<FilePath>(){File(targetDir + "/" + Config.UnitTests.UnitTestProjectName + ".dll")};
    OpenCoverSettings settings = new OpenCoverSettings();
    settings.ArgumentCustomization = args => args.Append(string.Concat("-targetdir:\"" + targetDir + "\""));
    settings.ArgumentCustomization = args => args.Append(string.Concat("-register")); // Magic!
    OpenCover(tool => {
        tool.XUnit2(
            targetDLLs,
            new XUnit2Settings {
                OutputDirectory = Config.ProjectInfo.ProjectDirectory,
                XmlReport = true,
                Parallelism = ParallelismOption.All, // Like Sanic
                ShadowCopy = false
            });
        },
        Config.UnitTests.CoverageReportFilePath,
        settings.WithFilter("+[" + Config.ProjectInfo.ProjectName + "*]*")
    );
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

Task("Pack-Nuget-Package")
    .WithCriteria(() => DirectoryExists(Config.Nuget.CreateNugetPackage))
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Pack Nuget Package.");
    }

    var nuGetPackSettings   = new NuGetPackSettings {
        Id                          = Config.Nuget.Id ?? "TestNuget",
        Version                     = Config.Nuget.Version ?? "0.0.0.1",
        Title                       = Config.Nuget.Title ?? "The tile of the package",
        Authors                     = (Config.Nuget.Authors ?? new[] {"John Doe"}).ToList(),
        Owners                      = (Config.Nuget.Owners ?? new[] {"Contoso"}).ToList(),
        Description                 = Config.Nuget.Description ?? "The description of the package",
        Summary                     = Config.Nuget.Summary ?? "Excellent summary of what the package does",
        ProjectUrl                  = Config.Nuget.ProjectUrl ?? new Uri("https://github.com/SomeUser/TestNuget/"),
        IconUrl                     = Config.Nuget.IconUrl ?? new Uri("http://cdn.rawgit.com/SomeUser/TestNuget/master/icons/testnuget.png"),
        LicenseUrl                  = Config.Nuget.LicenseUrl ?? new Uri("https://github.com/SomeUser/TestNuget/blob/master/LICENSE.md"),
        Copyright                   = Config.Nuget.Copyright ?? "Some company 2015",
        ReleaseNotes                = (Config.Nuget.ReleaseNotes ?? new [] {"Bug fixes", "Issue fixes", "Typos"}).ToList(),
        Tags                        = (Config.Nuget.Tags ?? new [] {"Cake", "Script", "Build"}).ToList(),
        RequireLicenseAcceptance    = Config.Nuget.RequireLicenseAcceptance,
        Symbols                     = Config.Nuget.Symbols,
        NoPackageAnalysis           = Config.Nuget.NoPackageAnalysis,
        Files                       = (List<NuSpecContent>)(Config.Nuget.Files
                                        // we want a null here if it is null
                                        // ?? new [] {
                                        //     new NuSpecContent {Source = "bin/TestNuget.dll", Target = "bin"},
                                        // }
                                        ),
        BasePath                    = Config.Nuget.BasePath ?? "./src/TestNuget/bin/release",
        OutputDirectory             = Config.Nuget.OutputDirectory ?? "./nuget",
        IncludeReferencedProjects   = Config.Nuget.IncludeReferencedProjects
    };

    Context.NuGetPack(Config.Nuget.PackPath ?? "./nuspec/TestNuget.nuspec", nuGetPackSettings);
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure nuspec is possible",
            "Ensure the nuget server is up",
            "Ensure nuget got installed"
        },
        true
        );
});

Task("Deploy-Nuget-Package")
    .WithCriteria(() => DirectoryExists(Config.Nuget.CreateNugetPackage))
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Deploy Nuget Package.");
    }

    NuGetPush(Config.Nuget.PackPath, new NuGetPushSettings {
        Source = Config.Nuget.ServerFeed ?? "http://example.com/nugetfeed",
        ApiKey = Config.Nuget.ApiKey
    });
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