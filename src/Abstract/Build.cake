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
    .IsDependentOn("DotNetCore-Run-Unit-Test")
    // .IsDependentOn("Start-SonarQube")
    // .IsDependentOn("TypeScriptCompile")   
    // .IsDependentOn("SassCompile")   
    .IsDependentOn("Raw-Build-Project")    
    // .IsDependentOn("End-SonarQube")
    // .IsDependentOn("Check-Quality-Gate")
    .IsDependentOn("Cleanup-Environment")
    // .IsDependentOn("CopyOutputToLocalDirectory")
    // .IsDependentOn("DeleteRemoteDir")
    // .IsDependentOn("UploadDir")
    // .IsDependentOn("SendAnAirbrakeDeploy")
    .IsDependentOn("DotNetCorePackNugetPackage")
    .IsDependentOn("DotNetCoreDeployNugetPackage")
;

Task("Setup-Environment")
    .Does(() =>
    {
        // setting the build directory
        Config.ConfigurableSettings.LocalCopyTargetDirectory = @"";
        Config.ConfigurableSettings.SpecificWebsiteOutputDir = @"";

        Config.ConfigurableSettings.DoLocalCopyWork = false;
        Config.ConfigurableSettings.DoFtpWork = false;

        Config.Slack.SlackChannel = "";
        Config.Slack.PostSlackStartAndStop = false;
        Config.Slack.PostSlackSteps = false;
        Config.Slack.PostSlackErrors = false;

        Config.ConfigurableSettings.DeleteLocalCopyDirBeforeCopy = false;

        Config.Nuget.CreateNugetPackage = Config.ProjectInfo.IsProduction;
        Config.Nuget.BuildForPack = false;
        Config.Nuget.Server = "https://www.nuget.org/";

        Config.MSBuildInfo.TargetFramework = "netstandard2.0";
        Config.MSBuildInfo.NoIncremental = true;

        Config.UnitTests.TestBlame = true;
        // Config.UnitTests.ListTests = true;
        Config.UnitTests.TargetFramework = "netcoreapp2.0";
    });

Task("Cleanup-Environment")
    .Does(() =>
    {
        // some cleanup stuff
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Context.Argument("target", "Build"));