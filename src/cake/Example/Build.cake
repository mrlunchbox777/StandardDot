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
    //.IsDependentOn("Build-Unit-Tests")
    //.IsDependentOn("Run-Unit-Tests")
    //.IsDependentOn("Start-SonarQube")
    .IsDependentOn("TypeScriptCompile")    
    .IsDependentOn("SassCompile")    
    .IsDependentOn("Build-Project")    
    //.IsDependentOn("End-SonarQube")
    //.IsDependentOn("Check-Quality-Gate")
    .IsDependentOn("Cleanup-Environment")
    .IsDependentOn("CopyOutputToLocalDirectory")
;

Task("Setup-Environment")
    //.IsDependentOn("CopyWebConfig")
    .Does(() =>
    {
        // setting the build directory
        cakeConfig.ConfigurableSettings.localCopyTargetDirectory = "";
        cakeConfig.ConfigurableSettings.specificWebsiteOutputDir = "";
    });

Task("Cleanup-Environment")
    //.IsDependentOn("RemoveWebConfig")
    .Does(() =>
    {
        //cakeConfig.CakeMethods.CopyFolderFromProjectRootToOutput(cakeConfig, "Scripts");
        // some cleanup stuff

    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Context.Argument("target", "Build"));