function ChangeBuildMe {
    param (
        [string]$folder
    )
    

    Set-Location "./$folder";
    $currentDir = Get-Location;
    $fullDir = $currentDir.ToString() + "/build.me";
    Write-Host "$fullDir";
    if (Test-Path "$fullDir")
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

Set-Location "./../";