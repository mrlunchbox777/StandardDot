//////////////////////////////////////////////////////////////////////////////////////
//                                     CAKE FILE                                    //
//                                                                                  //
//  If in doubt, use C#! All standard libraries are automatically imported, and if  //
//  not just add via a using statement as you would normally!                       //
//  A quick overview of the DSL (anything that looks syntactically off) can be      //
//  found here:                                                                     //
//      http://cakebuild.net/docs                                                   //
//  Common IO operations included in Cake can be found here:                        //
//      http://cakebuild.net/dsl                                                    //
//                                                                                  //
//////////////////////////////////////////////////////////////////////////////////////


// Install addins.
#addin nuget:?package=Polly // For timeout / retry
#addin "nuget:?package=NuGet.Core"
// #addin Cake.Ftp
#addin "nuget:?package=FluentFTP"
#addin "Cake.Slack"
#addin "Cake.Http"

// Install tools. We aren't running tests right now
// #tool "nuget:?package=SonarQube.MSBuild.Runner"
// #tool "nuget:?package=OpenCover"

// Load other scripts.
#load "Cake/CakeConfig.cake"

// Using statements
using Polly;
using FluentFTP;
using System.Net;
using System.Xml;
using System.IO;
using System.Environment;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Security;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

public CakeConfig Config;
private string _nugetInfo;
private string _slackInfo;

private bool? _stepUpADirectoryInConfigureSpace = null;

Setup(context =>
{
    if (context == null)
    {
        throw new InvalidOperationException("Context is null.");
    }
    bool stepUpADirectoryInConfigureSpace = true;
    try {
        if ((((bool?)_stepUpADirectoryInConfigureSpace) != default(bool?)) && ((bool)(_stepUpADirectoryInConfigureSpace ?? true) == default(bool))){
            stepUpADirectoryInConfigureSpace = (bool)_stepUpADirectoryInConfigureSpace;
        }
    } catch (Exception) {}
    Information("Step up a directory in configure space = " + stepUpADirectoryInConfigureSpace);
    // Do init here
    Config = new CakeConfig(context, true, true, stepUpADirectoryInConfigureSpace);
    _nugetInfo = ((!string.IsNullOrEmpty(Config.Nuget.Description) ? Config.Nuget.Description : "Didn't work"));
    _slackInfo = (Config.ProjectInfo.EnvironmentName) + " Deploy - " + Config.ProjectInfo.ProjectName + ". Other Info - " + _nugetInfo;
    Information("--------------------------------------------------------------------------------");
    Information("BUILD info = " + _nugetInfo + " version - " + Config.Nuget.Version + ".");
    Information("--------------------------------------------------------------------------------");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Starting-Up-Notification")
    .WithCriteria(() => Config.Slack.PostSlackStartAndStop)
    .Does(() =>
{
    Config.CakeMethods.SendSlackNotification(Config, "Starting " + _slackInfo);
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Failed to send a slack message, is the hook uri correct?",
            "Failed to send a slack message, is slack down?"
        },
        true
        );
});


//////////////////////////////////////////////////////////////////////
// LOCAL COPY
//////////////////////////////////////////////////////////////////////

Task("Copy-Output-To-Local-Directory")
    .WithCriteria(() => Config.ConfigurableSettings.DoLocalCopyWork)
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Copy Output To Local Directory.");
    }

    if (string.IsNullOrWhiteSpace(Config.ConfigurableSettings.LocalCopyTargetDirectory))
    {
        throw new CakeException("No local copy target directory variable set. Please set - 'Config.ConfigurableSettings.localCopyTargetDirectory' - in your build.");
    }

    if (Config.ConfigurableSettings.DeleteLocalCopyDirBeforeCopy && DirectoryExists(Config.ConfigurableSettings.LocalCopyTargetDirectory))
    {
        DeleteDirectory(Config.ConfigurableSettings.LocalCopyTargetDirectory, true);
    }

    EnsureDirectoryExists(Config.ConfigurableSettings.LocalCopyTargetDirectory);
    string sourceDir = Config.ProjectInfo.FlattenOutputDirectory + "\\" + Config.ConfigurableSettings.SpecificWebsiteOutputDir;
    Information("--------------------------------------------------------------------------------");
    Information("Starting Local Copy");
    Information("--------------------------------------------------------------------------------");
    CopyDirectory(sourceDir, Config.ConfigurableSettings.LocalCopyTargetDirectory);
    Information("--------------------------------------------------------------------------------");
    Information("Local Copy Complete");
    Information("--------------------------------------------------------------------------------");
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'Config.ConfigurableSettings.LocalCopyTargetDirectory' is set in the Config.ConfigurableSettings"
        },
        true
        );
});

//////////////////////////////////////////////////////////////////////
// FTP COPY
//////////////////////////////////////////////////////////////////////

Task("Delete-Remote-Dir")
  .WithCriteria(() => Config.ConfigurableSettings.DoFtpWork)
  .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Delete Remote Directory.");
    }

    Information("--------------------------------------------------------------------------------");
    Information("Deleting the remote directory");
    Information("--------------------------------------------------------------------------------");
    FtpClient client = new FtpClient(Config.FtpHelper.Host);
    var secure = Config.FtpHelper.DecryptPassword(Config.FtpHelper.SecurePasswordLocation, Config.FtpHelper.AesKey);
    client.Credentials = new NetworkCredential(Config.FtpHelper.Username, secure);
    client.SocketPollInterval = Config.FtpHelper.SocketPollInterval;
    client.ConnectTimeout = Config.FtpHelper.ConnectTimeout;
    client.ReadTimeout = Config.FtpHelper.ReadTimeout;
    client.DataConnectionConnectTimeout = Config.FtpHelper.DataConnectionConnectTimeout;
    client.DataConnectionReadTimeout = Config.FtpHelper.DataConnectionReadTimeout;
    client.RetryAttempts = Config.FtpHelper.DeleteRetryAttempts;
    client.Connect();

    try
    {
        client.DeleteDirectory(Config.FtpHelper.RemoteDir, FtpListOption.AllFiles);
    } catch (FluentFTP.FtpCommandException ex)
    {
        // if we couldn't find it, that's ok
        if (!ex.Message.Contains("cannot find the"))
        {
            client.Disconnect();
            throw;
        }
    }

    client.Disconnect();
    Information("--------------------------------------------------------------------------------");
    Information("Deleted the remote directory");
    Information("--------------------------------------------------------------------------------");
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'Config.FtpHelper.RemoteDir' is set in the Config.FtpHelper"
        },
        true
        );
});

Task("Upload-Dir")
  .WithCriteria(() => Config.ConfigurableSettings.DoFtpWork)
  .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting FTP Upload.");
    }

    Information("--------------------------------------------------------------------------------");
    Information("Uploading a directory");
    Information("--------------------------------------------------------------------------------");
    string sourceDir = Config.ProjectInfo.FlattenOutputDirectory + "\\" + Config.ConfigurableSettings.SpecificWebsiteOutputDir;
    FtpClient client = new FtpClient(Config.FtpHelper.Host);
    var secure = Config.FtpHelper.DecryptPassword(Config.FtpHelper.SecurePasswordLocation, Config.FtpHelper.AesKey);
    client.Credentials = new NetworkCredential(Config.FtpHelper.Username, secure);
    client.SocketPollInterval = Config.FtpHelper.SocketPollInterval;
    client.ConnectTimeout = Config.FtpHelper.ConnectTimeout;
    client.ReadTimeout = Config.FtpHelper.ReadTimeout;
    client.DataConnectionConnectTimeout = Config.FtpHelper.DataConnectionConnectTimeout;
    client.DataConnectionReadTimeout = Config.FtpHelper.DataConnectionReadTimeout;
    client.RetryAttempts = Config.FtpHelper.UploadRetryAttempts;
    client.Connect();

    try
    {
        Config.CakeMethods.UploadDirectory(Config, client, sourceDir, sourceDir);
    } catch (Exception)
    {
        client.Disconnect();
    }

    client.Disconnect();
    Information("--------------------------------------------------------------------------------");
    Information("Uploaded a directory");
    Information("--------------------------------------------------------------------------------");
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'Config.FtpHelper.RemoteDir' is set in the Config.FtpHelper"
        },
        true
        );
});

//////////////////////////////////////////////////////////////
// SonarQube Tasks
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