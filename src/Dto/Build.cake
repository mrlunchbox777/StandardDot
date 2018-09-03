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

#load "../StandardDot.cake"

Task("Bake-Cake")
    .IsDependentOn("Setup-Environment")
    .IsDependentOn("Starting-Up-Notification")    
    // .IsDependentOn("DotNet-Core-Run-Unit-Test")
    // .IsDependentOn("Start-SonarQube")
    // .IsDependentOn("Type-Script-Compile")   
    // .IsDependentOn("Sass-Compile")   
    .IsDependentOn("Build-Project-DotNet-Core")    
    // .IsDependentOn("Update-Version-From-Assembly")    
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
    .IsDependentOn("Common-Setup-Environment")
    .Does(() =>
    {
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