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
    string nugetInfo = ("Building || " + (!string.IsNullOrEmpty(cakeConfig.Nuget.nugetDescription) ? cakeConfig.Nuget.nugetDescription : "Didn't work"));
    Information("--------------------------------------------------------------------------------");
    Information("BUILD info = " + nugetInfo + " version - " + cakeConfig.ProjectInfo.projectVersion + ".");
    Information("--------------------------------------------------------------------------------");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////////////////////////////
// LOCAL COPY
//////////////////////////////////////////////////////////////////////

// make sure to set this in your cakeConfig.ConfigurableSettings
// cakeConfig.ConfigurableSettings.localCopyTargetDirectory = "";
// cakeConfig.ConfigurableSettings.specificWebsiteOutputDir = ""

Task("CopyOutputToLocalDirectory")
    .Does(() =>
{
    if (string.IsNullOrWhiteSpace(cakeConfig.ConfigurableSettings.localCopyTargetDirectory))
    {
        throw new CakeException("No local copy target directory variable set. Please set - 'cakeConfig.ConfigurableSettings.localCopyTargetDirectory' - in your build.");
    }
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
    Information("--------------------------------------------------------------------------------");
    Information("Deleting the remote directory");
    Information("--------------------------------------------------------------------------------");
    FtpClient client = new FtpClient(cakeConfig.ConfigurableSettings.ftpHost);
    var secure = new SecureString();
    foreach (char c in System.IO.File.ReadAllText(cakeConfig.ConfigurableSettings.ftpSecurePasswordLocation))
    {
        secure.AppendChar(c);
    }
    client.Credentials = new NetworkCredential(cakeConfig.ConfigurableSettings.ftpUsername, secure);
    client.Connect();
    client.DeleteDirectory(cakeConfig.ConfigurableSettings.ftpRemoteDir);
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
    Information("--------------------------------------------------------------------------------");
    Information("Uploading a directory");
    Information("--------------------------------------------------------------------------------");
    string sourceDir = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.specificWebsiteOutputDir;
    FtpClient client = new FtpClient(cakeConfig.ConfigurableSettings.ftpHost);
    var secure = new SecureString();
    foreach (char c in System.IO.File.ReadAllText(cakeConfig.ConfigurableSettings.ftpSecurePasswordLocation))
    {
        secure.AppendChar(c);
    }
    client.Credentials = new NetworkCredential(cakeConfig.ConfigurableSettings.ftpUsername, secure);
    client.Connect();
    client.RetryAttempts = 3;
    cakeConfig.CakeMethods.UploadDirectory(cakeConfig, client, sourceDir, sourceDir);
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