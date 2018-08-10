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
    .IsDependentOn("Build-Unit-Tests")
    .IsDependentOn("Run-Unit-Tests")
    // .IsDependentOn("Start-SonarQube")
    // .IsDependentOn("TypeScriptCompile")   
    // .IsDependentOn("SassCompile")   
    .IsDependentOn("Build-Project")    
    // .IsDependentOn("End-SonarQube")
    // .IsDependentOn("Check-Quality-Gate")
    .IsDependentOn("Cleanup-Environment")
    // .IsDependentOn("CopyOutputToLocalDirectory")
    // .IsDependentOn("DeleteRemoteDir")
    // .IsDependentOn("UploadDir")
    // .IsDependentOn("SendAnAirbrakeDeploy")
    .IsDependentOn("PackNugetPackage")
    .IsDependentOn("DeployNugetPackage")
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
        Config.Nuget.PackPath = "./src/Abstract/Abstract.nuspec";
        Config.Nuget.Id = "StandardDot.Abstract";
        Config.Nuget.Version = "0.0.0.1";
        Config.Nuget.Title = "StandardDot.Abstract";
        Config.Nuget.Authors = new List<string>() { "Andrew Shoell" };
        Config.Nuget.Owners = new List<string>() { "Andrew Shoell" };
        Config.Nuget.Description = "Abstractions for the Standard Dot Libraries.";
        Config.Nuget.Summary = "Abstractions for the Standard Dot Libraries.";
        Config.Nuget.ProjectUrl = new Uri("https://github.com/mrlunchbox777/StandardDot");
        Config.Nuget.IconUrl = new Uri("https://github.com/mrlunchbox777/StandardDot/blob/master/StandardDotIcon.png");
        Config.Nuget.LicenseUrl = new Uri("https://github.com/mrlunchbox777/StandardDot/blob/master/LICENSE.md");
        Config.Nuget.Copyright = "Copyright (c) 2018 Andrew Shoell";
        Config.Nuget.ReleaseNotes = new List<string>() { "Intial Release" };
        Config.Nuget.Tags = new List<string>() { "Library" };
        Config.Nuget.RequireLicenseAcceptance = false;
        Config.Nuget.Symbols = false;
        Config.Nuget.NoPackageAnalysis = false;
        // Config.Nuget.Files = new List<NuSpecContent>()
        //     {
        //         new NuSpecContent { Source = "bin/TestNuget.dll", Target = "bin" }
        //     };
        // Config.Nuget.BasePath = "";
        // Config.Nuget.OutputDirectory = "";
        Config.Nuget.IncludeReferencedProjects = true;
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