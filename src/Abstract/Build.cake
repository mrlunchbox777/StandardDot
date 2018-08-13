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

#load "../Cake/CSharp/MainDotNetCoreTasks.cake"

Task("Bake-Cake")
    .IsDependentOn("Setup-Environment")
    .IsDependentOn("Starting-Up-Notification")    
    .IsDependentOn("DotNet-Core-Run-Unit-Test")
    // .IsDependentOn("Start-SonarQube")
    // .IsDependentOn("Type-Script-Compile")   
    // .IsDependentOn("Sass-Compile")   
    .IsDependentOn("Build-Project-DotNet-Core")    
    .IsDependentOn("Update-Version-From-Assembly")    
    // .IsDependentOn("End-SonarQube")
    // .IsDependentOn("Check-Quality-Gate")
    .IsDependentOn("Cleanup-Environment")
    // .IsDependentOn("Copy-Output-To-Local-Directory")
    // .IsDependentOn("Delete-Remote-Dir")
    // .IsDependentOn("Upload-Dir")
    // .IsDependentOn("Send-An-Airbrake-Deploy")
    .IsDependentOn("DotNet-Core-Pack-Nuget-Package")
    .IsDependentOn("DotNet-Core-Deploy-Nuget-Package")
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
        // Config.Nuget.Version = null;

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