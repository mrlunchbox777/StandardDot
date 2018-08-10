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
    // .IsDependentOn("Build-Unit-Tests")
    // .IsDependentOn("Run-Unit-Tests")
    // .IsDependentOn("Start-SonarQube")
    // .IsDependentOn("TypeScriptCompile")   
    // .IsDependentOn("SassCompile")   
    .IsDependentOn("Build-Project")    
    // .IsDependentOn("End-SonarQube")
    // .IsDependentOn("Check-Quality-Gate")
    .IsDependentOn("Cleanup-Environment")
    .IsDependentOn("CopyOutputToLocalDirectory")
    .IsDependentOn("DeleteRemoteDir")
    .IsDependentOn("UploadDir")
    .IsDependentOn("SendAnAirbrakeDeploy")
    // .IsDependentOn("PackNugetPackage")
    // .IsDependentOn("DeployNugetPackage")
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

        Config.Nuget.CreateNugetPackage = false;
        // Config.Nuget.PackPath = "";
        Config.Nuget.Id = "";
        Config.Nuget.Version = "0.0.0.1";
        // Config.Nuget.Title = "";
        Config.Nuget.Authors = new List<string>() { "" };
        Config.Nuget.Owners = new List<string>() { "" };
        Config.Nuget.Description = "";
        Config.Nuget.Summary = "";
        // Config.Nuget.ProjectUrl = new Uri("");
        // Config.Nuget.IconUrl = new Uri("");
        // Config.Nuget.LicenseUrl = new Uri("");
        Config.Nuget.Copyright = "";
        Config.Nuget.ReleaseNotes = new List<string>() { "" };
        Config.Nuget.Tags = new List<string>() { "" };
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