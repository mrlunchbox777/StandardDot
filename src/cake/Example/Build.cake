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

#load "../Cake/Main.cake" // Main Cake File
#load "../Cake/CSharp/MainTasks.cake"

Task("Bake-Cake")
    .IsDependentOn("Setup-Environment")
    .IsDependentOn("StartingUpNotification")    
    //.IsDependentOn("Build-Unit-Tests")
    //.IsDependentOn("Run-Unit-Tests")
    //.IsDependentOn("Start-SonarQube")
    //.IsDependentOn("TypeScriptCompile")   
    //.IsDependentOn("SassCompile")   
    .IsDependentOn("Build-Project")    
    //.IsDependentOn("End-SonarQube")
    //.IsDependentOn("Check-Quality-Gate")
    .IsDependentOn("Cleanup-Environment")
    .IsDependentOn("CopyOutputToLocalDirectory")
    .IsDependentOn("DeleteRemoteDir")
    .IsDependentOn("UploadDir")
    .IsDependentOn("SendAnAirbrakeDeploy")
;

Task("Setup-Environment")
    .IsDependentOn("CopyWebConfig")
    .Does(() =>
    {
        // setting the build directory
        Config.ConfigurableSettings.LocalCopyTargetDirectory = @"";
        Config.ConfigurableSettings.SpecificWebsiteOutputDir = @"";

        Config.FtpHelper.Host = "";
        Config.FtpHelper.RemoteDir = "";
        Config.FtpHelper.Username = "";
        Config.FtpHelper.SecurePasswordLocation = @"";

        Config.ConfigurableSettings.DoLocalCopyWork = !Config.ProjectInfo.IsProduction;
        Config.ConfigurableSettings.DoFtpWork = Config.ProjectInfo.IsProduction;

        Config.Slack.SlackChannel = "";
        Config.Slack.PostSlackStartAndStop = true;
        Config.Slack.PostSlackSteps = true;
        Config.Slack.PostSlackErrors = true;

        Config.ConfigurableSettings.DeleteLocalCopyDirBeforeCopy = true;

        Config.Airbrake.ProjectId = "";
        Config.Airbrake.UserName = "";
        Config.Airbrake.Email = "";
        Config.Airbrake.ProjectKey = "";
        
        Config.ConfigurableSettings.ApplicationPoolName = "";
        Config.ConfigurableSettings.ApplicationSiteName = "";
        Config.ConfigurableSettings.RestartIIS = true;
        Config.ConfigurableSettings.UseRemoteServer = false;
        Config.ConfigurableSettings.RemoteIISServerName = "";
    });

Task("Cleanup-Environment")
    .IsDependentOn("RemoveWebConfig")
    // .IsDependentOn("CopyWebConfigToOutput")
    .Does(() =>
    {
        Config.CakeMethods.CopyFolderFromProjectRootToOutput(Config, "Content");
        Config.CakeMethods.CopyFolderFromProjectRootToOutput(Config, "fonts");
        // some cleanup stuff
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Context.Argument("target", "Build"));