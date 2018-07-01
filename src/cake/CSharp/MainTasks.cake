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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Restore Packages.");
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
            NuGetRestore(cakeConfig.ProjectInfo.projectSolution, new NuGetRestoreSettings {
                // we don't want to define a source atm
                // Source = new List<string> {
                //     cakeConfig.Nuget.nugetServerURL
                // },
                ToolTimeout = TimeSpan.FromMinutes(toolTimeout),
                PackagesDirectory = cakeConfig.Nuget.packagesDirectory
            });
        });
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Build Project.");
    }
    if (cakeConfig.MSBuildInfo.shouldFlatten(false))
    {
        MSBuild(cakeConfig.ProjectInfo.projectFile, new MSBuildSettings()
            //.WithTarget(cakeConfig.ProjectInfo.projectName) //.Replace('.','_')
            .SetConfiguration("Release")
            .WithProperty("Platform", cakeConfig.MSBuildInfo.platform)        
            .WithProperty("VisualStudioVersion", cakeConfig.MSBuildInfo.MSBuildVersion)
            .WithProperty("PipelineDependsOnBuild", "false")
            .WithProperty("OutputPath", cakeConfig.ProjectInfo.FlattenOutputDirectory)
            .WithProperty("ExcludeFilesFromDeployment", "\"**\\*.svn\\**\\*.*;Web.*.config;*.cs;*\\*.cs;*\\*\\*.cs;*\\*\\*\\*.cs;*.csproj\"")
            .UseToolVersion(MSBuildToolVersion.Default)
            .SetVerbosity(Verbosity.Minimal)
            .SetMaxCpuCount(1));
    }
    else
    {
        MSBuild(cakeConfig.ProjectInfo.projectFile, new MSBuildSettings()
            //.WithTarget(cakeConfig.ProjectInfo.projectName) //.Replace('.','_')
            .SetConfiguration(cakeConfig.MSBuildInfo.msbuildConfig(false))
            .WithProperty("Platform", cakeConfig.MSBuildInfo.platform)        
            .WithProperty("VisualStudioVersion", cakeConfig.MSBuildInfo.MSBuildVersion)
            .UseToolVersion(MSBuildToolVersion.Default)
            .SetVerbosity(Verbosity.Minimal)
            .SetMaxCpuCount(1));
    }
    
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Copy Web Config.");
    }
    // get the web.config
    string origWebConfigLocation = cakeConfig.ProjectInfo.projectDirectory + "\\Web.config.example";
    string newWebConfigLocation = cakeConfig.ProjectInfo.projectDirectory + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Copying - " + origWebConfigLocation + " -> " + newWebConfigLocation);
    Information("--------------------------------------------------------------------------------");
    CopyFile(origWebConfigLocation, newWebConfigLocation);
})

    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Copy Web Config To Output.");
    }
    // get the web.config
    string origWebConfigLocation = cakeConfig.ProjectInfo.projectDirectory + "\\Web.config";
    string newWebConfigLocation = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.specificWebsiteOutputDir + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Copying - " + origWebConfigLocation + " -> " + newWebConfigLocation);
    Information("--------------------------------------------------------------------------------");
    CopyFile(origWebConfigLocation, newWebConfigLocation);
})

    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Remove Web Config.");
    }
    // remove the web.config
    string webConfigLocation = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.specificWebsiteOutputDir + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Deleting - " + webConfigLocation);
    Information("--------------------------------------------------------------------------------");
    DeleteFile(webConfigLocation);
})

    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Sass Compile.");
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
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Type Script Compile.");
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
    cakeConfig.DispalyException(
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
    .WithCriteria(() => DirectoryExists(cakeConfig.UnitTests.unitTestDirectoryPath))
    .IsDependentOn("Restore-CSharp-NuGet-Packages")
    .Does(() =>
{
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Build Unit Tests.");
    }
    MSBuild(cakeConfig.ProjectInfo.projectSolution, new MSBuildSettings()
        .WithTarget(cakeConfig.UnitTests.unitTestProjectName.Replace('.','_'))
        .SetConfiguration(cakeConfig.MSBuildInfo.msbuildConfig(true))
        .WithProperty("Platform", cakeConfig.MSBuildInfo.platform)        
        .WithProperty("Configuration", cakeConfig.MSBuildInfo.msbuildConfig(true))
        .WithProperty("VisualStudioVersion", cakeConfig.MSBuildInfo.MSBuildVersion)
        .UseToolVersion(MSBuildToolVersion.Default)
        .SetVerbosity(Verbosity.Minimal)
        .SetMaxCpuCount(1));
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    .WithCriteria(() => DirectoryExists(cakeConfig.UnitTests.unitTestDirectoryPath))
    .IsDependentOn("Build-Unit-Tests")
    .Does(() =>
{
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Run Unit Tests.");
    }
    string targetDir = cakeConfig.UnitTests.unitTestDirectoryPath.FullPath + "/bin/" + cakeConfig.MSBuildInfo.msbuildConfig(true);
    IEnumerable<FilePath> targetDLLs = new List<FilePath>(){File(targetDir + "/" + cakeConfig.UnitTests.unitTestProjectName + ".dll")};
    OpenCoverSettings settings = new OpenCoverSettings();
    settings.ArgumentCustomization = args => args.Append(string.Concat("-targetdir:\"" + targetDir + "\""));
    settings.ArgumentCustomization = args => args.Append(string.Concat("-register")); // Magic!
    OpenCover(tool => {
        tool.XUnit2(
            targetDLLs,
            new XUnit2Settings {
                OutputDirectory = cakeConfig.ProjectInfo.projectDirectory,
                XmlReport = true,
                Parallelism = ParallelismOption.All, // Like Sanic
                ShadowCopy = false
            });
        },
        cakeConfig.UnitTests.coverageReportFilePath,
        settings.WithFilter("+[" + cakeConfig.ProjectInfo.projectName + "*]*")
    );
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Stopping IIS.");
    }
    if (cakeConfig.ConfigurableSettings.restartIIS) {
        if (cakeConfig.ConfigurableSettings.useRemoteServer)
        {
            StopPool(cakeConfig.ConfigurableSettings.remoteIISServerName, cakeConfig.ConfigurableSettings.applicationPoolName);
            StopSite(cakeConfig.ConfigurableSettings.remoteIISServerName, cakeConfig.ConfigurableSettings.applicationSiteName);
        } else
        {
            StopPool(cakeConfig.ConfigurableSettings.applicationPoolName);
            StopSite(cakeConfig.ConfigurableSettings.applicationSiteName);
        }
    }
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting IIS.");
    }
    if (cakeConfig.ConfigurableSettings.restartIIS) {
        if (cakeConfig.ConfigurableSettings.useRemoteServer)
        {
            StartPool(cakeConfig.ConfigurableSettings.remoteIISServerName, cakeConfig.ConfigurableSettings.applicationPoolName);
            StartSite(cakeConfig.ConfigurableSettings.remoteIISServerName, cakeConfig.ConfigurableSettings.applicationSiteName);
        } else
        {
            StartPool(cakeConfig.ConfigurableSettings.applicationPoolName);
            StartSite(cakeConfig.ConfigurableSettings.applicationSiteName);
        }
    }
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting SonarQube.");
    }
    // using (var process = StartAndReturnProcess(
    //     "./tools/SonarQube.MSBuild.Runner/tools/MSBuild.SonarQube.Runner.exe", 
    //     new ProcessSettings()
    //         .WithArguments(
    //             arguments => {
    //                 arguments
    //                     .Append("begin")
    //                     .AppendSwitchQuoted(@"/k", ":", cakeConfig.ProjectInfo.projectName)
    //                     .AppendSwitchQuoted(@"/n", ":", cakeConfig.ProjectInfo.projectName)
    //                     .AppendSwitchQuoted(@"/v", ":", cakeConfig.ProjectInfo.projectVersion);
    //                 if (!string.IsNullOrEmpty(EnvironmentVariable("SONARQUBE_KEY")))
    //                 {
    //                     arguments
    //                         .AppendSwitchQuoted(@"/d", ":", "sonar.login=" + EnvironmentVariable("SONARQUBE_KEY"));
    //                 }
    //                 if (DirectoryExists(cakeConfig.UnitTests.unitTestDirectoryPath))
    //                 {
    //                     arguments
    //                         .AppendSwitchQuoted(@"/d", ":", "sonar.cs.opencover.reportsPaths=" + cakeConfig.UnitTests.coverageReportFilePath)
    //                         .AppendSwitchQuoted(@"/d", ":", "sonar.cs.xunit.reportsPaths=" + cakeConfig.UnitTests.xUnitOutputFile);
    //                 }
    //                 if (!string.IsNullOrEmpty(cakeConfig.UnitTests.jsTestPath))
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
    cakeConfig.DispalyException(
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Complete SonarQube Analysis.");
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
    //     cakeConfig.UnitTests.sqAnalysisUrl = GetSonarQubeURL(System.IO.File.ReadAllLines(filename));
    //     DeleteFile(filename);
    //     Information("--------------------------------------------------------------------------------");
    //     Information("Check " + cakeConfig.UnitTests.sqAnalysisUrl + " for a sonarqube update status.");
    //     Information("--------------------------------------------------------------------------------");
    //}
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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
    .WithCriteria(() => !String.IsNullOrEmpty(cakeConfig.UnitTests.sqAnalysisUrl))
    .Does(() => 
{
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Check Quality Gate.");
    }
    // cakeConfig.UnitTests.qualityGateReady = IsAnalysisComplete(cakeConfig.UnitTests.sqAnalysisUrl);
    // int timeoutCount = 0;
    // while(!cakeConfig.UnitTests.qualityGateReady) // Giving it up to two minutes to complete
    // {
    //     if (cakeConfig.UnitTests.maxQualityGateTimeoutCount < timeoutCount) throw new CakeException("Could not get quality gate from SonarQube");
    //     cakeConfig.UnitTests.qualityGateReady = IsAnalysisComplete(cakeConfig.UnitTests.sqAnalysisUrl);
    //     System.Threading.Thread.Sleep(cakeConfig.UnitTests.QualityGateSleepLengthPerCount);
    //     timeoutCount++;
    // }
    // cakeConfig.UnitTests.qualityGateStatus = CheckQualityGate(cakeConfig.ProjectInfo.projectName);
    // if (string.IsNullOrEmpty(cakeConfig.UnitTests.qualityGateStatus))
    // {
    //     Environment.Exit(1);
    // }
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
        exception,
        new string[] {
            "Ensure sonarqube is available and not too busy",
            "Try again... the server can get overloaded"
        },
        false
        );
});