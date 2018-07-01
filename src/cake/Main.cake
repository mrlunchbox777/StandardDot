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

public CakeConfig cakeConfig;
private string nugetInfo;
private string slackInfo;

bool? StepUpADirectoryInConfigureSpace = null;

Setup(context =>
{
    bool stepUpADirectoryInConfigureSpace = true;
    try {
        if ((((bool?)StepUpADirectoryInConfigureSpace) != default(bool?)) && ((bool)(StepUpADirectoryInConfigureSpace ?? true) == default(bool))){
            stepUpADirectoryInConfigureSpace = (bool)StepUpADirectoryInConfigureSpace;
        }
    } catch (Exception) {}
    Information("Step up a directory in configure space = " + stepUpADirectoryInConfigureSpace);
    // Do init here
    cakeConfig = new CakeConfig(context, true, true, stepUpADirectoryInConfigureSpace);
    nugetInfo = ("Guru Technologies || " + (!string.IsNullOrEmpty(cakeConfig.Nuget.nugetDescription) ? cakeConfig.Nuget.nugetDescription : "Didn't work"));
    slackInfo = (cakeConfig.ProjectInfo.EnvironmentName) + " Deploy - " + cakeConfig.ProjectInfo.projectName + ". Other Info - " + nugetInfo;
    Information("--------------------------------------------------------------------------------");
    Information("BUILD info = " + nugetInfo + " version - " + cakeConfig.ProjectInfo.projectVersion + ".");
    Information("--------------------------------------------------------------------------------");
});

Teardown(context =>
{
    if (cakeConfig.ConfigurableSettings.postSlackStartAndStop)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Finished " + slackInfo);
    }
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////



Task("StartingUpNotification")
    .Does(() =>
{
    if (cakeConfig.ConfigurableSettings.postSlackStartAndStop)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting " + slackInfo);
    }

})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
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

// make sure to set this in your cakeConfig.ConfigurableSettings
// cakeConfig.ConfigurableSettings.localCopyTargetDirectory = "";
// cakeConfig.ConfigurableSettings.specificWebsiteOutputDir = ""

Task("CopyOutputToLocalDirectory")
    .Does(() =>
{
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Copy Output To Local Directory.");
    }
    if (!cakeConfig.ConfigurableSettings.doLocalCopyWork)
    {
        return;
    }
    if (string.IsNullOrWhiteSpace(cakeConfig.ConfigurableSettings.localCopyTargetDirectory))
    {
        throw new CakeException("No local copy target directory variable set. Please set - 'cakeConfig.ConfigurableSettings.localCopyTargetDirectory' - in your build.");
    }
    if (cakeConfig.ConfigurableSettings.deleteLocalCopyDirBeforeCopy && DirectoryExists(cakeConfig.ConfigurableSettings.localCopyTargetDirectory))
    {
        DeleteDirectory(cakeConfig.ConfigurableSettings.localCopyTargetDirectory, true);
    }
    EnsureDirectoryExists(cakeConfig.ConfigurableSettings.localCopyTargetDirectory);
    string sourceDir = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.specificWebsiteOutputDir;
    Information("--------------------------------------------------------------------------------");
    Information("Starting Local Copy");
    Information("--------------------------------------------------------------------------------");
    CopyDirectory(sourceDir, cakeConfig.ConfigurableSettings.localCopyTargetDirectory);
    Information("--------------------------------------------------------------------------------");
    Information("Local Copy Complete");
    Information("--------------------------------------------------------------------------------");
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'cakeConfig.ConfigurableSettings.localCopyTargetDirectory' is set in the cakeConfig.ConfigurableSettings"
        },
        true
        );
});


//////////////////////////////////////////////////////////////////////
// FTP COPY
//////////////////////////////////////////////////////////////////////

// make sure to set this in your cakeConfig.ConfigurableSettings
// cakeConfig.ConfigurableSettings.ftpHost = "";
// cakeConfig.ConfigurableSettings.ftpRemoteDir = ""
// cakeConfig.ConfigurableSettings.ftpUsername = ""
// cakeConfig.ConfigurableSettings.ftpSecurePasswordLocation = ""

Task("DeleteRemoteDir")
  .Does(() => {
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Delete Remote Directory.");
    }
    if (!cakeConfig.ConfigurableSettings.doFtpWork)
    {
        return;
    }
    Information("--------------------------------------------------------------------------------");
    Information("Deleting the remote directory");
    Information("--------------------------------------------------------------------------------");
    FtpClient client = new FtpClient(cakeConfig.ConfigurableSettings.ftpHost);
    var secure = cakeConfig.FtpHelper.DecryptPassword(cakeConfig.ConfigurableSettings.ftpSecurePasswordLocation, cakeConfig.ConfigurableSettings.ftpAesKey);
    client.Credentials = new NetworkCredential(cakeConfig.ConfigurableSettings.ftpUsername, secure);
    client.SocketPollInterval = cakeConfig.ConfigurableSettings.ftpSocketPollInterval;
    client.ConnectTimeout = cakeConfig.ConfigurableSettings.ftpConnectTimeout;
    client.ReadTimeout = cakeConfig.ConfigurableSettings.ftpReadTimeout;
    client.DataConnectionConnectTimeout = cakeConfig.ConfigurableSettings.ftpDataConnectionConnectTimeout;
    client.DataConnectionReadTimeout = cakeConfig.ConfigurableSettings.ftpDataConnectionReadTimeout;
    client.RetryAttempts = cakeConfig.ConfigurableSettings.ftpDeleteRetryAttempts;
    client.Connect();
    try
    {
        client.DeleteDirectory(cakeConfig.ConfigurableSettings.ftpRemoteDir, FtpListOption.AllFiles);
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
    cakeConfig.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'cakeConfig.ConfigurableSettings.ftpRemoteDir' is set in the cakeConfig.ConfigurableSettings"
        },
        true
        );
});

Task("UploadDir")
  .Does(() => {
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting FTP Upload.");
    }
    if (!cakeConfig.ConfigurableSettings.doFtpWork)
    {
        return;
    }
    Information("--------------------------------------------------------------------------------");
    Information("Uploading a directory");
    Information("--------------------------------------------------------------------------------");
    string sourceDir = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.specificWebsiteOutputDir;
    FtpClient client = new FtpClient(cakeConfig.ConfigurableSettings.ftpHost);
    var secure = cakeConfig.FtpHelper.DecryptPassword(cakeConfig.ConfigurableSettings.ftpSecurePasswordLocation, cakeConfig.ConfigurableSettings.ftpAesKey);
    client.Credentials = new NetworkCredential(cakeConfig.ConfigurableSettings.ftpUsername, secure);
    client.SocketPollInterval = cakeConfig.ConfigurableSettings.ftpSocketPollInterval;
    client.ConnectTimeout = cakeConfig.ConfigurableSettings.ftpConnectTimeout;
    client.ReadTimeout = cakeConfig.ConfigurableSettings.ftpReadTimeout;
    client.DataConnectionConnectTimeout = cakeConfig.ConfigurableSettings.ftpDataConnectionConnectTimeout;
    client.DataConnectionReadTimeout = cakeConfig.ConfigurableSettings.ftpDataConnectionReadTimeout;
    client.RetryAttempts = cakeConfig.ConfigurableSettings.ftpUploadRetryAttempts;
    client.Connect();
    try
    {
        cakeConfig.CakeMethods.UploadDirectory(cakeConfig, client, sourceDir, sourceDir);
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
    cakeConfig.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'cakeConfig.ConfigurableSettings.ftpRemoteDir' is set in the cakeConfig.ConfigurableSettings"
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
    if (cakeConfig.ConfigurableSettings.postSlackSteps)
    {
        cakeConfig.CakeMethods.SendSlackNotification(cakeConfig, "Starting Send An Airbrake Deploy.");
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
    settings.SetRequestBody("{\"environment\":\"" + cakeConfig.ProjectInfo.EnvironmentName + "\",\"username\":\""
        + cakeConfig.ConfigurableSettings.airbrakeUserName + "\",\"email\":\"" + cakeConfig.ConfigurableSettings.airbrakeEmail + "\","
        + "\"repository\":\"" + EnvironmentVariable("CI_REPOSITORY_URL") + "/" + EnvironmentVariable("CI_COMMIT_REF_NAME") + "\",\"revision\":\""
        + EnvironmentVariable("CI_COMMIT_SHA") + "\",\"version\":\"v2.0\"}");

    string responseBody = HttpPost("https://airbrake.io/api/v4/projects/" + cakeConfig.ConfigurableSettings.airbrakeProjectId
        + "/deploys?key=" + cakeConfig.Airbrake.AirbrakeProjectKey, settings);
    Information("--------------------------------------------------------------------------------");
    Information("Send An Airbrake Deploy Complete");
    Information("--------------------------------------------------------------------------------");
})
    .ReportError(exception =>
{
    cakeConfig.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'cakeConfig.ConfigurableSettings.localCopyTargetDirectory' is set in the cakeConfig.ConfigurableSettings"
        },
        true
        );
});