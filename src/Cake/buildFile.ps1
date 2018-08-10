function Bake-A-Cake {
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$false)]
        [AllowNull()]
        [String]
        $PSScriptRoot,

        [Parameter(Mandatory=$false)]
        [AllowNull()]
        [String]
        $CakeTarget,
        
        [Parameter(Mandatory=$false)]
        [AllowNull()]
        [String]
        $CakeDirectory
    )

    Write-Host "Current script root - " $PSScriptRoot
    if ([string]::IsNullOrEmpty($PSScriptRoot))
    {
        Write-Host "Modifying script root"
        $PSScriptRoot = (Get-Location).Path
        
        if ((Test-Path $PSScriptRoot) -and ((Get-ChildItem -Path "$PSScriptRoot" | Where-Object {$_.Extension -eq ".cake"} | Measure).Count -gt 0))
        {
            Write-Host "Picking parent folder because no cake file was found in root"
            $PSScriptRoot = Split-Path -Path $PSScriptRoot -Parent
        }
    }
    Write-Host "Final script root - " $PSScriptRoot

    $env:HasRunChocolatelyUpdate = $false

    # get changes
    $diff = @();

    if ([System.String]::isNullOrEmpty($env:CI_COMMIT_REF_NAME) -or (($env:CI_COMMIT_REF_NAME -ne "master") -and ($env:CI_COMMIT_REF_NAME -ne "develop")))
    {
        exit;
    }

    if (-not [System.String]::isNullOrEmpty($env:CI_COMMIT_SHA))
    {
        Write-Host "Found git commits, running commit work."
        $allParents = iex "& git rev-list --first-parent $env:CI_COMMIT_SHA";

        ForEach ($thing in & 'git' diff --no-commit-id --name-only -r $allParents[1])
        { # Gets every file from commit and adds it to $diff
            $diff += $thing; 
        }
    }
    else
    {
        Write-Host "Found no git commits, running " + $CakeTarget +" work."
    }

    $env:CAKE_ROSLYN_NUGETSOURCE = "https://www.nuget.org"
    #(Bake-Cake, Half-Baked)
    $Target = "Bake-Cake"
    $Configuration = "Release"
    #("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")
    $Verbosity = "Diagnostic"

    # run if there are changes
    $alreadyBuilt = @(); # Memoize so we don't build twice
    ForEach ($change in $diff)
    {
        Write-Host $change
        if((-not [System.String]::isNullOrEmpty($change)) `
        -and (!$change.Contains('Tests')) `
        -and (!$change.Contains(".sql")) 
        )
        {
            $PROJECTNAME = $change.Split('/')[0]
            if ($alreadyBuilt.Contains($PROJECTNAME))
            {
                continue
            }
            else
            {
                $alreadyBuilt += ($PROJECTNAME)
            }

            FindAndRunCakeScript $PROJECTNAME $CakeTarget $Target $Configuration $Verbosity $PROJECTNAME $PSScriptRoot
        }
    }
    
    # run if this isn't change driven
    if ([System.String]::isNullOrEmpty($env:CI_COMMIT_SHA))
    {
        # indexing -1 is the last element
        $PROJECTNAME = $CakeDirectory.Split('/')[-1]
        $PROJECTNAME = $PROJECTNAME.Split('\')[-1]
        FindAndRunCakeScript $CakeDirectory $CakeTarget $Target $Configuration $Verbosity $PROJECTNAME $PSScriptRoot
    }
}

function EnsureJava () {
    # JAVA_HOME for SonarQube
    Write-Host "JAVA_HOME = $env:JAVA_HOME"
    $jreRegex = "j((dk)|(re))(?<majorVersion>[\d\.]+)?(?<minorVersion>_[\d]*)"
    if ([System.String]::IsNullOrEmpty($env:JAVA_HOME) -or $env:JAVA_HOME -notmatch $jreRegex)
    {
        Write-Host "JRE not found, trying to locate..."
        $javaPath = "C:\Program Files\Java"
        if (Test-Path "C:\Program Files (x86)\Java")
        {
            $javaPath = "C:\Program Files (x86)\Java"
            Get-ChildItem -Path "C:\Program Files (x86)\Java" | Where-Object {$_.BaseName -match $jreRegex} | Out-Null
        }
        if (Test-Path "C:\Program Files\Java")
        {
            $javaPath = "C:\Program Files\Java"
            Get-ChildItem -Path "C:\Program Files\Java" | Where-Object {$_.BaseName -match $jreRegex} | Out-Null
        }    
        $majorVersion = ($Matches['majorVersion'] | Measure-Object -Maximum).Maximum
        $minVersion = ($Matches['minorVersion'] | Measure-Object -Maximum).Maximum
        $highestJavaPath = "$javaPath\jre$($majorVersion)_$($minVersion)"
        if (Test-Path $highestJavaPath)
        {
            [System.Environment]::SetEnvironmentVariable("JAVA_HOME", $highestJavaPath, [System.EnvironmentVariableTarget]::Process)
            Write-Host "JAVA_HOME = $env:JAVA_HOME"
        }
        else
        {
            Write-Host "JAVA_HOME not found, downloading JAVA"
            EnsureChocolatey
            $existingPaths = $Env:Path -Split ';' | Where-Object { ![System.String]::IsNullOrEmpty($_) } | Where-Object { Test-Path $_ }

            # Try find some java
            Write-Verbose -Message "Trying to find java.exe in PATH..."
            $Java_EXE_IN_PATH = Get-ChildItem -Path $existingPaths | Where-Object {$_.Name -eq "java.exe"} | Select -First 1
            if ($Java_EXE_IN_PATH -ne $null -and (Test-Path $Java_EXE_IN_PATH.FullName))
            {
                Write-Verbose -Message "Found in PATH at $($Java_EXE_IN_PATH.FullName). Updating...."
                
                # Updating that sweet, sweet java
                Write-Verbose -Message "Updating Java..."
                try
                {
                    choco.exe upgrade jdk8 -y
                }
                catch
                {
                    Throw "Could not update Java."
                }
            }
            else
            {
                # Download that sweet, sweet java
                Write-Verbose -Message "Downloading Java..."
                try
                {
                    choco.exe install jdk8 -y
                }
                catch
                {
                    Throw "Could not download Java."
                }
            }
            
            # Cool refresh PATH
            RefreshEnv.cmd
        }
    }
}

function FindAndRunCakeScript ([string]$CAKEDIR, [string]$CakeTarget, [string]$Target, [string]$Configuration, [string]$Verbosity, [string]$PROJECTNAME, [string]$BaseDirToLink) {
    $Script = ""
    Write-Host ("Looking for a .cake file in " + "$CAKEDIR" + ".")
    EnsureTypeScript
    
    $NewCakeDir = "C:\devLink"
    if(Test-Path $NewCakeDir)
    {
        Write-Host "Found a symLink, removing..."
        cmd /c rmdir "$NewCakeDir"
    }

    if (Test-Path $CAKEDIR)
    {
        Write-Host "Creating Symbolic Link (Junction)"
        cmd /c mklink /j "$NewCakeDir" "$BaseDirToLink"
        if (!($BaseDirToLink -eq $CAKEDIR))
        {
            Write-Host "Adding the project directory to the junction cake dir"
            $NewCakeDir = Join-Path $NewCakeDir $PROJECTNAME
        }
    }
    else
    {
        Write-Host "Can't find cake dir, skipping link"
    }

    if(!(Test-Path $NewCakeDir))
    {
        Write-Host "Creating link $NewCakeDir didn't work...."
    }
    else
    {
        Write-Host "Creating link successful - $NewCakeDir"
    }

    if ((Test-Path $NewCakeDir) -and ((Get-ChildItem -Path "$NewCakeDir" | Where-Object {$_.Extension -eq ".cake"} | Measure).Count -gt 0))
    {
        if ([System.String]::isNullOrEmpty($CakeTarget) -or($CakeTarget.Contains('Build')))
        {
            Write-Host "Looking for Build.Cake"

            # we are going to need this for sonarqube
            #EnsureJava
            $env:WORKSPACE = "C:\devLink"
            $Script = (Get-ChildItem -Path "$NewCakeDir" | Where-Object {$_.Name -eq "Build.cake"})[0].FullName
        }
        else
        {
            Write-Host "Can't find Cakefile for " + "$CakeTarget" + " in " + "$CAKEDIR" + "... Abandoning ship!"
            continue
        }
    }
    if ([System.String]::isNullOrEmpty($Script))
    {
        Write-Host "Can't find Cakefile in " + "$CAKEDIR" + "... Abandoning ship!"
        continue
    }

    Write-Host ("Preparing to run build script at " + "$Script" + "...")
    EnsureNugetAndCake

    # Start Cake
    Write-Host "Running build script..."
    Write-Host "& `"$ENV:CAKE_EXE`" `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" --roslyn_nugetsource=`"$env:CAKE_ROSLYN_NUGETSOURCE`" -project=`"$PROJECTNAME`" -experimental=`"true`""
    $allOutput;
    Invoke-Expression ("& `"$ENV:CAKE_EXE`" `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" --roslyn_nugetsource=`"$env:CAKE_ROSLYN_NUGETSOURCE`" -project=`"$PROJECTNAME`" -experimental=`"true`" 2>&1") 2>&1
    if ($LASTEXITCODE) {
        Write-Host "Found error, exiting. - " + $LASTEXITCODE + " error - " + $allOutput
        EXIT $LASTEXITCODE;
    } else {
        Write-Host "No error found"
    }
}

function EnsureNugetAndCake () {
    #("DryRun","Noop")
    #$SkipToolPackageRestore = $false

    #$PSScriptRoot = split-path -parent $MyInvocation.MyCommand.Definition;
    $WORKING_DIR = $MyInvocation.MyCommand.Path;
            
    $TOOLS_DIR = Join-Path $PSScriptRoot "tools"
    $ENV:CAKE_EXE = Join-Path $TOOLS_DIR "Cake/Cake.exe"
    $NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
    $NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    $PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"

    # Make sure tools folder exists
    if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR))
    {
        Write-Verbose -Message "Creating tools directory..."
        New-Item -Path $TOOLS_DIR -Type directory | out-null
    }

    # Make sure that packages.config exist.
    if (!(Test-Path $PACKAGES_CONFIG))
    {
        Write-Verbose -Message "Downloading packages.config..."
        try
        {
            #Copy-Item (Join-Path $PSScriptRoot "/packages.config") $TOOLS_DIR
            Invoke-WebRequest -Uri http://cakebuild.net/download/bootstrapper/packages -OutFile $PACKAGES_CONFIG
        }
        catch
        {
            Throw "Could not download packages.config."
        }
    }

    # Download that sweet, sweet nuget
    if (!(Test-Path $NUGET_EXE))
    {
            Write-Host "Nuget.exe not found, downloading Nuget"
            EnsureChocolatey
            $existingPaths = $Env:Path -Split ';' | Where-Object { ![System.String]::IsNullOrEmpty($_) } | Where-Object { Test-Path $_ }

            # Try find some nuget
            Write-Verbose -Message "Trying to find nuget.exe in PATH..."
            $Nuget_EXE_IN_PATH = Get-ChildItem -Path $existingPaths | Where-Object {$_.Name -eq "nuget.exe"} | Select -First 1
            if ($Nuget_EXE_IN_PATH -ne $null -and (Test-Path $Nuget_EXE_IN_PATH.FullName))
            {
                Write-Verbose -Message "Found in PATH at $($Nuget_EXE_IN_PATH.FullName). Updating...."
                
                # Updating that sweet, sweet nuget
                Write-Verbose -Message "Updating Nuget..."
                try
                {
                    choco.exe upgrade nuget.commandline -y
                }
                catch
                {
                    Throw "Could not update Nuget."
                }
            }
            else
            {
                # Download that sweet, sweet nuget
                Write-Verbose -Message "Downloading Nuget..."
                try
                {
                    choco.exe install nuget.commandline -y
                }
                catch
                {
                    Throw "Could not download Nuget."
                }
            }
            
            # Cool refresh PATH
            RefreshEnv.cmd
    }

    # Try find some chocolatey nuget in our path
    if (!(Test-Path $NUGET_EXE))
    {
        Write-Verbose -Message "Trying to find nuget.exe in PATH..."
        $existingPaths = $Env:Path -Split ';' | Where-Object { ![System.String]::IsNullOrEmpty($_) } | Where-Object { Test-Path $_ }
        $NUGET_EXE_IN_PATH = Get-ChildItem -Path $existingPaths | Where-Object {$_.Name -eq "nuget.exe"} | Select -First 1
        if ($NUGET_EXE_IN_PATH -ne $null -and (Test-Path $NUGET_EXE_IN_PATH.FullName))
        {
            Write-Verbose -Message "Found in PATH at $($NUGET_EXE_IN_PATH.FullName)."
            $NUGET_EXE = $NUGET_EXE_IN_PATH.FullName
        }
    }

    # Save nuget.exe path to environment to be available to child processes
    $ENV:NUGET_EXE = $NUGET_EXE

    # Restore tools from NuGet?
    if(-Not $SkipToolPackageRestore.IsPresent)
    {
        Push-Location
        Set-Location $TOOLS_DIR
        Write-Verbose -Message "Restoring tools from NuGet..."
        $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$TOOLS_DIR`""
        # nuget puts out a different code if everything is installed, and it'll error later
        # if ($LASTEXITCODE -ne 0)
        # {
        #    Throw "An error occured while restoring NuGet tools."
        # }
        Write-Verbose -Message ($NuGetOutput | out-string)
        Pop-Location
    }
}

function EnsureTypeScript (){
    Write-Host "Checking for the typescript compiler"
    EnsureChocolatey
    $existingPaths = $Env:Path -Split ';' | Where-Object { ![System.String]::IsNullOrEmpty($_) } | Where-Object { Test-Path $_ }

    # Try find some Typescript
    Write-Verbose -Message "Trying to find tsc.exe in PATH..."
    $Tsc_EXE_IN_PATH = Get-ChildItem -Path $existingPaths | Where-Object {($_ -match ".*tsc.exe") -or ($_ -match ".*tsc.cmd")} | Select -First 1
    if ($Tsc_EXE_IN_PATH -ne $null -and (Test-Path $Tsc_EXE_IN_PATH.FullName))
    {
        Write-Verbose -Message "Found in PATH at $($Tsc_EXE_IN_PATH.FullName). Updating...."
        
        # Updating that sweet, sweet Typescript
        Write-Verbose -Message "Updating Typescript..."
        try
        {
            choco.exe upgrade typescript -y
        }
        catch
        {
            Throw "Could not update Typescript."
        }
    }
    else
    {
        # Download that sweet, sweet Typescript
        Write-Verbose -Message "Downloading Typescript..."
        try
        {
            choco.exe install typescript -y
        }
        catch
        {
            Throw "Could not download Typescript."
        }
    }
    npm install typescript -g
}
function EnsureChocolatey() {
    if($env:HasRunChocolatelyUpdate){}
    else
    {
        # Try find some chocolatey nuget in our path
        Write-Verbose -Message "Trying to find choco.exe in PATH..."
        $existingPaths = $Env:Path -Split ';' | Where-Object { ![System.String]::IsNullOrEmpty($_) } | Where-Object { Test-Path $_ }
        $Choco_EXE_IN_PATH = Get-ChildItem -Path $existingPaths | Where-Object {$_.Name -eq "choco.exe"} | Select -First 1
        if ($Choco_EXE_IN_PATH -ne $null -and (Test-Path $Choco_EXE_IN_PATH.FullName))
        {
            Write-Verbose -Message "Found in PATH at $($Choco_EXE_IN_PATH.FullName). Updating..."
            
            # Updating that sweet, sweet chocolatey
            Write-Verbose -Message "Updating Choco..."
            try
            {
                choco.exe upgrade chocolatey -y
            }
            catch
            {
                Throw "Could not update Choco."
            }
        }
        else
        {
            # Download that sweet, sweet choco
            Write-Verbose -Message "Downloading Choco.exe..."
            try
            {
                iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
            }
            catch
            {
                Throw "Could not download choco.exe."
            }
        }

        # Manually refresh PATH
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

        # Cool refresh PATH
        RefreshEnv.cmd

        # Only run choco download/upgrade once per run
        $env:HasRunChocolatelyUpdate = $true
    }
}