#addin nuget:?package=Polly // For timeout / retry
#addin "nuget:?package=NuGet.Core"
//#addin "Cake.Powershell"
#addin "Cake.IIS"
#addin "nuget:?package=System.ServiceProcess.ServiceController"

//#tool "nuget:?package=Microsoft.TypeScript.Compiler&version=2.7.2"

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

Task("Raw-Build-Project")
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
        + (string.IsNullOrWhiteSpace(Config.Nuget.VersionSuffix) ? "" : " --version-suffix " + Config.Nuget.VersionSuffix)
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

Task("Build-Project")
    .IsDependentOn("Restore-CSharp-NuGet-Packages")
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

Task("CopyWebConfig")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Copy Web Config.");
    }
    // get the web.config
    string origWebConfigLocation = Config.ProjectInfo.ProjectDirectory + "\\Web.config.example";
    string newWebConfigLocation = Config.ProjectInfo.ProjectDirectory + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Copying - " + origWebConfigLocation + " -> " + newWebConfigLocation);
    Information("--------------------------------------------------------------------------------");
    CopyFile(origWebConfigLocation, newWebConfigLocation);
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});

Task("CopyWebConfigToOutput")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Copy Web Config To Output.");
    }
    // get the web.config
    string origWebConfigLocation = Config.ProjectInfo.ProjectDirectory + "\\Web.config";
    string newWebConfigLocation = Config.ProjectInfo.FlattenOutputDirectory + "\\" + Config.ConfigurableSettings.SpecificWebsiteOutputDir + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Copying - " + origWebConfigLocation + " -> " + newWebConfigLocation);
    Information("--------------------------------------------------------------------------------");
    CopyFile(origWebConfigLocation, newWebConfigLocation);
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});

Task("RemoveWebConfig")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Remove Web Config.");
    }
    // remove the web.config
    string webConfigLocation = Config.ProjectInfo.FlattenOutputDirectory + "\\" + Config.ConfigurableSettings.SpecificWebsiteOutputDir + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Deleting - " + webConfigLocation);
    Information("--------------------------------------------------------------------------------");
    DeleteFile(webConfigLocation);
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});

Task("SassCompile")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Sass Compile.");
    }
    Information("--------------------------------------------------------------------------------");
    Information("Compiling Sass - ");
    Information("--------------------------------------------------------------------------------");
    try {
        StartProcess("..\\scss_compiler.bat");
    } catch (Exception ex) {
        Information("Got and ignoring error - " + ex.Message + ".\r\nStack trace -\r\n" + ex.StackTrace);
    }
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});

Task("TypeScriptCompile")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Type Script Compile.");
    }
    Information("--------------------------------------------------------------------------------");
    Information("Compiling TSC - ");
    Information("--------------------------------------------------------------------------------");
    try {
        //StartPowershellScript("C:\\Windows\\System32\\config\\systemprofile\\AppData\\Roaming\\npm\\tsc.cmd");
        StartProcess("C:\\Windows\\System32\\config\\systemprofile\\AppData\\Roaming\\npm\\tsc.cmd");
    } catch (Exception ex) {
        Information("Got and ignoring error - " + ex.Message + ".\r\nStack trace -\r\n" + ex.StackTrace);
    }
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});


//////////////////////////////////////////////////////////////
// Unit Test Tasks (we aren't doing any of these right now)
//////////////////////////////////////////////////////////////

Task("DotNetCore-Run-Unit-Test")
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
            + (string.IsNullOrWhiteSpace(Config.MSBuildInfo.TargetFramework) ? "" : " -f " + Config.MSBuildInfo.TargetFramework)
            + (string.IsNullOrWhiteSpace(Config.UnitTests.FilterExpression) ? "" : " --filter " + Config.UnitTests.FilterExpression)
            + (Config.UnitTests.NoBuildForTest ? " --no-build" : "")
            + (Config.UnitTests.NoRestoreForTest ? " --no-restore" : "")
            + (Config.MSBuildInfo.ShouldFlatten() ? " -o \"" + Config.ProjectInfo.FlattenOutputDirectory + "\"" : "")
            + " -r " + (string.IsNullOrWhiteSpace(Config.UnitTests.ResultsDirectory) ? Config.ProjectInfo.ProjectDirectory : Config.UnitTests.ResultsDirectory)
            + (string.IsNullOrWhiteSpace(Config.UnitTests.SettingsFile) ? "" : " -s " + Config.UnitTests.SettingsFile)
            + (Config.UnitTests.ListTests ? " -t" : "")
            + (string.IsNullOrWhiteSpace(Config.Nuget.VerbosityLevel) ? "" : " -v " + Config.Nuget.VerbosityLevel)
        );
    }
    catch (Exception)
    {
        Config.MSBuildInfo.IsRunningTests = true;
        throw;
    }
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

Task("StopAnApplicationPool")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Stopping IIS.");
    }
    if (Config.ConfigurableSettings.RestartIIS) {
        if (Config.ConfigurableSettings.UseRemoteServer)
        {
            StopPool(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationPoolName);
            StopSite(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationSiteName);
        } else
        {
            StopPool(Config.ConfigurableSettings.ApplicationPoolName);
            StopSite(Config.ConfigurableSettings.ApplicationSiteName);
        }
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
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting IIS.");
    }
    if (Config.ConfigurableSettings.RestartIIS) {
        if (Config.ConfigurableSettings.UseRemoteServer)
        {
            StartPool(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationPoolName);
            StartSite(Config.ConfigurableSettings.RemoteIISServerName, Config.ConfigurableSettings.ApplicationSiteName);
        } else
        {
            StartPool(Config.ConfigurableSettings.ApplicationPoolName);
            StartSite(Config.ConfigurableSettings.ApplicationSiteName);
        }
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
// SonarQube Tasks (We aren't going to use these right now)
//////////////////////////////////////////////////////////////

Task("Start-SonarQube")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting SonarQube.");
    }
    // using (var process = StartAndReturnProcess(
    //     "./tools/SonarQube.MSBuild.Runner/tools/MSBuild.SonarQube.Runner.exe", 
    //     new ProcessSettings()
    //         .WithArguments(
    //             arguments => {
    //                 arguments
    //                     .Append("begin")
    //                     .AppendSwitchQuoted(@"/k", ":", Config.ProjectInfo.ProjectName)
    //                     .AppendSwitchQuoted(@"/n", ":", Config.ProjectInfo.ProjectName)
    //                     .AppendSwitchQuoted(@"/v", ":", Config.Nuget.Version);
    //                 if (!string.IsNullOrEmpty(EnvironmentVariable("SONARQUBE_KEY")))
    //                 {
    //                     arguments
    //                         .AppendSwitchQuoted(@"/d", ":", "sonar.login=" + EnvironmentVariable("SONARQUBE_KEY"));
    //                 }
    //                 if (DirectoryExists(Config.UnitTests.UnitTestDirectoryPath))
    //                 {
    //                     arguments
    //                         .AppendSwitchQuoted(@"/d", ":", "sonar.cs.opencover.reportsPaths=" + Config.UnitTests.CoverageReportFilePath)
    //                         .AppendSwitchQuoted(@"/d", ":", "sonar.cs.xunit.reportsPaths=" + Config.UnitTests.XUnitOutputFile);
    //                 }
    //                 if (!string.IsNullOrEmpty(Config.UnitTests.JsTestPath))
    //                 {
    //                     arguments
    //                         .AppendSwitchQuoted("/d",":", "sonar.javascript.lcov.reportPath=jsTests.lcov");
    //                 }
    //             }   
    //             )
    //         )
    //     )
    // {
    //     process.WaitForExit();
    //     if (process.GetExitCode() != 0) throw new CakeException("Could not start SonarQube analysis");
    // }
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure java is installed on the machine",
            "ENSURE THE UNIT TESTS HAVE AT LEAST 1 XUNIT TEST",
            "Check for file locks"
        },
        true
        );
});

Task("End-SonarQube")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Complete SonarQube Analysis.");
    }
    // using (var process = StartAndReturnProcess(
    //         "./tools/SonarQube.MSBuild.Runner/tools/MSBuild.SonarQube.Runner.exe", 
    //         new ProcessSettings()
    //             .SetRedirectStandardOutput(true)
    //             .WithArguments(
    //                 arguments => {
    //                     arguments.Append("end");
    //                     }
    //                 )
    //             )
    //         )
    // {
    //     Information("--------------------------------------------------------------------------------");
    //     Information("Starting stdout capture");
    //     Information("--------------------------------------------------------------------------------");
    //     process.WaitForExit();
    //     IEnumerable<string> stdout = process.GetStandardOutput();
    //     Information("Aggregating.....");      
    //     string filename = string.Format("reallyLameFileToNeed{0}.txt",Guid.NewGuid());  
    //     System.IO.File.WriteAllLines(filename, stdout);
    //     Config.UnitTests.SqAnalysisUrl = GetSonarQubeURL(System.IO.File.ReadAllLines(filename));
    //     DeleteFile(filename);
    //     Information("--------------------------------------------------------------------------------");
    //     Information("Check " + Config.UnitTests.SqAnalysisUrl + " for a sonarqube update status.");
    //     Information("--------------------------------------------------------------------------------");
    //}
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure java is installed on the machine",
            "ENSURE THE UNIT TESTS HAVE AT LEAST 1 XUNIT TEST",
            "Check for file locks"
        },
        true
        );
});

Task("Check-Quality-Gate")
    .WithCriteria(() => !String.IsNullOrEmpty(Config.UnitTests.SqAnalysisUrl))
    .Does(() => 
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Check Quality Gate.");
    }
    // Config.UnitTests.QualityGateReady = IsAnalysisComplete(Config.UnitTests.SqAnalysisUrl);
    // int timeoutCount = 0;
    // while(!Config.UnitTests.QualityGateReady) // Giving it up to two minutes to complete
    // {
    //     if (Config.UnitTests.MaxQualityGateTimeoutCount < timeoutCount) throw new CakeException("Could not get quality gate from SonarQube");
    //     Config.UnitTests.QualityGateReady = IsAnalysisComplete(Config.UnitTests.SqAnalysisUrl);
    //     System.Threading.Thread.Sleep(Config.UnitTests.QualityGateSleepLengthPerCount);
    //     timeoutCount++;
    // }
    // Config.UnitTests.QualityGateStatus = CheckQualityGate(Config.ProjectInfo.ProjectName);
    // if (string.IsNullOrEmpty(Config.UnitTests.QualityGateStatus))
    // {
    //     Environment.Exit(1);
    // }
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure sonarqube is available and not too busy",
            "Try again... the server can get overloaded"
        },
        false
        );
});

//////////////////////////////////////////////////////////////
// Deploy Nuget
//////////////////////////////////////////////////////////////

Task("PackNugetPackage")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Pack Nuget Package.");
    }
    if (!Config.Nuget.CreateNugetPackage)
    {
        return;
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

Task("DeployNugetPackage")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Deploy Nuget Package.");
    }
    if (!Config.Nuget.CreateNugetPackage)
    {
        return;
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

Task("DotNetCorePackNugetPackage")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Pack Nuget Package.");
    }
    if (!Config.Nuget.CreateNugetPackage)
    {
        return;
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
        + " -o " + (string.IsNullOrWhiteSpace(Config.Nuget.PackPath) ? Config.ProjectInfo.ProjectDirectory : Config.Nuget.PackPath)
        + (string.IsNullOrWhiteSpace(Config.Nuget.RunTimeVersion) ? "" : " --runtime " + Config.Nuget.RunTimeVersion)
        + (Config.Nuget.Servicable ? " -s" : "")
        + (string.IsNullOrWhiteSpace(Config.Nuget.VersionSuffix) ? "" : " --version-suffix " + Config.Nuget.VersionSuffix)
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

Task("DotNetCoreDeployNugetPackage")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Deploy Nuget Package.");
    }
    if (!Config.Nuget.CreateNugetPackage)
    {
        return;
    }

    var packageFinder = (string.IsNullOrWhiteSpace(Config.Nuget.PackPath)
            ? Config.ProjectInfo.ProjectDirectory
            : Config.Nuget.PackPath)
        + ".*.nupkg";

    var package = GetFiles(packageFinder).FirstOrDefault();

    if (package == null)
    {
        throw new InvalidOperationException("Unable to find .nupkg.");
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