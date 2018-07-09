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
                Source = new List<string> {
                    Config.Nuget.Server
                },
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

Task("Build-Project")
    .IsDependentOn("Restore-CSharp-NuGet-Packages")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Build Project.");
    }
    if (Config.MSBuildInfo.ShouldFlatten(false))
    {
        MSBuild(Config.ProjectInfo.ProjectFile, new MSBuildSettings()
            //.WithTarget(Config.ProjectInfo.ProjectName) //.Replace('.','_')
            .SetConfiguration("Release")
            .WithProperty("Platform", Config.MSBuildInfo.Platform)        
            .WithProperty("VisualStudioVersion", Config.MSBuildInfo.MsBuildVersion)
            .WithProperty("PipelineDependsOnBuild", "false")
            .WithProperty("OutputPath", Config.ProjectInfo.FlattenOutputDirectory)
            .WithProperty("ExcludeFilesFromDeployment", "\"**\\*.svn\\**\\*.*;Web.*.config;*.cs;*\\*.cs;*\\*\\*.cs;*\\*\\*\\*.cs;*.csproj\"")
            .UseToolVersion(MSBuildToolVersion.Default)
            .SetVerbosity(Verbosity.Minimal)
            .SetMaxCpuCount(1));
    }
    else
    {
        MSBuild(Config.ProjectInfo.ProjectFile, new MSBuildSettings()
            //.WithTarget(Config.ProjectInfo.ProjectName) //.Replace('.','_')
            .SetConfiguration(Config.MSBuildInfo.MsBuildConfig(false))
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
        .SetConfiguration(Config.MSBuildInfo.MsBuildConfig(true))
        .WithProperty("Platform", Config.MSBuildInfo.Platform)        
        .WithProperty("Configuration", Config.MSBuildInfo.MsBuildConfig(true))
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
    string targetDir = Config.UnitTests.UnitTestDirectoryPath.FullPath + "/bin/" + Config.MSBuildInfo.MsBuildConfig(true);
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