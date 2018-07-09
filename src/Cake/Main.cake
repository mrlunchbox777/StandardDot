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
//#tool "nuget:?package=xunit.runner.console&version=2.1.0"
//#tool "nuget:?package=SonarQube.MSBuild.Runner"
//#tool "nuget:?package=OpenCover"

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

Teardown(context =>
{
    if (Config?.Slack?.PostSlackStartAndStop ?? false)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Finished " + _slackInfo);
    }
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////



Task("StartingUpNotification")
    .Does(() =>
{
    if (Config.Slack.PostSlackStartAndStop)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting " + _slackInfo);
    }

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

// make sure to set this in your Config.ConfigurableSettings
// Config.ConfigurableSettings.LocalCopyTargetDirectory = "";
// Config.ConfigurableSettings.SpecificWebsiteOutputDir = ""

Task("CopyOutputToLocalDirectory")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Copy Output To Local Directory.");
    }
    if (!Config.ConfigurableSettings.DoLocalCopyWork)
    {
        return;
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

// make sure to set this in your Config.ConfigurableSettings
// Config.FtpHelper.Host = "";
// Config.FtpHelper.RemoteDir = ""
// Config.FtpHelper.Username = ""
// Config.FtpHelper.SecurePasswordLocation = ""

Task("DeleteRemoteDir")
  .Does(() => {
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Delete Remote Directory.");
    }
    if (!Config.ConfigurableSettings.DoFtpWork)
    {
        return;
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

Task("UploadDir")
  .Does(() => {
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting FTP Upload.");
    }
    if (!Config.ConfigurableSettings.DoFtpWork)
    {
        return;
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

//////////////////////////////////////////////////////////////////////
// Airbrake Deploy
//////////////////////////////////////////////////////////////////////

Task("SendAnAirbrakeDeploy")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Send An Airbrake Deploy.");
    }

    Information("--------------------------------------------------------------------------------");
    Information("Starting Send An Airbrake Deploy");
    Information("--------------------------------------------------------------------------------");
    var settings = new HttpSettings
    {
        Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        },
    };
    settings.SetRequestBody("{\"environment\":\"" + Config.ProjectInfo.EnvironmentName + "\",\"username\":\""
        + Config.Airbrake.UserName + "\",\"email\":\"" + Config.Airbrake.Email + "\","
        + "\"repository\":\"" + EnvironmentVariable("CI_REPOSITORY_URL") + "/" + EnvironmentVariable("CI_COMMIT_REF_NAME") + "\",\"revision\":\""
        + EnvironmentVariable("CI_COMMIT_SHA") + "\",\"version\":\"v2.0\"}");

    string responseBody = HttpPost("https://airbrake.io/api/v4/projects/" + Config.Airbrake.ProjectId
        + "/deploys?key=" + Config.Airbrake.ProjectKey, settings);
    Information("--------------------------------------------------------------------------------");
    Information("Send An Airbrake Deploy Complete");
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