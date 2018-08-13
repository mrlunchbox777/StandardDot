function ChangeBuildMe {
    param (
        [string]$folder
    )
    

    Set-Location "./$folder";
    if ([System.IO.File]::Exists("build.me"))
    {
        Remove-Item "build.me";
    }
    else
    {
        New-Item "build.me";
    }
    Set-Location "./../";
}

Set-Location "./src";

ChangeBuildMe("Abstract");
ChangeBuildMe("Authentication");
ChangeBuildMe("Caching");
ChangeBuildMe("Configuration");
ChangeBuildMe("Constants");
ChangeBuildMe("CoreExtensions");
ChangeBuildMe("CoreServices");
ChangeBuildMe("Dto");
ChangeBuildMe("Enums");