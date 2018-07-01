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
        cakeConfig.ConfigurableSettings.localCopyTargetDirectory = @"";
        cakeConfig.ConfigurableSettings.specificWebsiteOutputDir = @"";

        cakeConfig.ConfigurableSettings.ftpHost = "";
        cakeConfig.ConfigurableSettings.ftpRemoteDir = "";
        cakeConfig.ConfigurableSettings.ftpUsername = "";
        cakeConfig.ConfigurableSettings.ftpSecurePasswordLocation = @"";

        cakeConfig.ConfigurableSettings.doLocalCopyWork = !cakeConfig.ProjectInfo.IsProduction;
        cakeConfig.ConfigurableSettings.doFtpWork = cakeConfig.ProjectInfo.IsProduction;

        cakeConfig.ConfigurableSettings.slackChannel = "";
        cakeConfig.ConfigurableSettings.postSlackStartAndStop = true;
        cakeConfig.ConfigurableSettings.postSlackSteps = true;
        cakeConfig.ConfigurableSettings.postSlackErrors = true;

        cakeConfig.ConfigurableSettings.deleteLocalCopyDirBeforeCopy = true;

        cakeConfig.ConfigurableSettings.airbrakeProjectId = "";
        cakeConfig.ConfigurableSettings.airbrakeUserName = "";
        cakeConfig.ConfigurableSettings.airbrakeEmail = "";
        cakeConfig.Airbrake.AirbrakeProjectKey = "";
        
        cakeConfig.ConfigurableSettings.applicationPoolName = "";
        cakeConfig.ConfigurableSettings.applicationSiteName = "";
        cakeConfig.ConfigurableSettings.restartIIS = true;
        cakeConfig.ConfigurableSettings.useRemoteServer = false;
        cakeConfig.ConfigurableSettings.remoteIISServerName = "";
    });

Task("Cleanup-Environment")
    .IsDependentOn("RemoveWebConfig")
    // .IsDependentOn("CopyWebConfigToOutput")
    .Does(() =>
    {
        cakeConfig.CakeMethods.CopyFolderFromProjectRootToOutput(cakeConfig, "Content");
        cakeConfig.CakeMethods.CopyFolderFromProjectRootToOutput(cakeConfig, "fonts");
        // some cleanup stuff
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Context.Argument("target", "Build"));