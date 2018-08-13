#!/usr/bin/env bash
joinPath()
{
    local BASEPATH="$1"
    local SUBDIR="$2"
    if [ -z $"BASEPATH" ]; then
        print $"SUBDIR"
        return
    fi
    if [ -z $"SUBDIR" ]; then
        print $"BASEPATH"
        return
    fi
    parts=("$BASEPATH" "$SUBDIR");
    printf '/%s' "${parts[@]%/}"
}

# Define default arguments.
SCRIPT="build.cake"
CAKE_ARGUMENTS=()
PSScriptRoot="${1:-${WORKSPACE:-default}}"
CakeTarget="${2:-Bake-Cake}"
CakeDirectory=$(joinPath $PSScriptRoot "Cake/")
CakeDirectory="${3:-${CakeDirectory:-default}}"

# Define directories.
SCRIPT_DIR="$PSScriptRoot"
TOOLS_DIR="$SCRIPT_DIR/tools"
ADDINS_DIR="$TOOLS_DIR/Addins"
MODULES_DIR="$TOOLS_DIR/Modules"
NUGET_EXE="$TOOLS_DIR/nuget.exe"
CAKE_DIR="$TOOLS_DIR/Cake"
CAKE_EXE="$TOOLS_DIR/Cake/Cake.exe"
PACKAGES_CONFIG="$TOOLS_DIR/packages.config"
PACKAGES_CONFIG_MD5="$TOOLS_DIR/packages.config.md5sum"
ADDINS_PACKAGES_CONFIG="$ADDINS_DIR/packages.config"
MODULES_PACKAGES_CONFIG="$MODULES_DIR/packages.config"

# Define md5sum or md5 depending on Linux/OSX
MD5_EXE=
if [[ "$(uname -s)" == "Darwin" ]]; then
    MD5_EXE="md5 -r"
else
    MD5_EXE="md5sum"
fi

##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

# Define functions
containsElement ()
{
  local e match="$1"
  shift
  for e; do [[ "$e" == "$match" ]] && return 0; done
  return 1
}

ensureCakeAndNuget ()
{
    echo "Script dir - $SCRIPT_DIR"
    # Make sure the tools folder exist.
    if [ ! -d "$TOOLS_DIR" ]; then
        mkdir "$TOOLS_DIR"
        echo "made tools dir - $TOOLS_DIR"
    fi

    # Make sure that packages.config exist.
    if [ ! -f "$TOOLS_DIR/packages.config" ]; then
        echo "Downloading packages.config..."
        curl -Lsfo "$TOOLS_DIR/packages.config" https://cakebuild.net/download/bootstrapper/packages
        if [ $? -ne 0 ]; then
            echo "An error occurred while downloading packages.config."
            exit 1
        fi
    fi

    # Download NuGet if it does not exist.
    if [ ! -f "$NUGET_EXE" ]; then
        echo "Downloading NuGet..."
        curl -Lsfo "$NUGET_EXE" https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
        if [ $? -ne 0 ]; then
            echo "An error occurred while downloading nuget.exe."
            exit 1
        fi
    fi

    # Restore tools from NuGet.
    pushd "$TOOLS_DIR" >/dev/null
    if [ ! -f "$PACKAGES_CONFIG_MD5" ] || [ "$( cat "$PACKAGES_CONFIG_MD5" | sed 's/\r$//' )" != "$( $MD5_EXE "$PACKAGES_CONFIG" | awk '{ print $1 }' )" ]; then
        find . -type d ! -name . ! -name 'Cake.Bakery' | xargs rm -rf
    fi

    mono "$NUGET_EXE" install -ExcludeVersion
    if [ $? -ne 0 ]; then
        echo "Could not restore NuGet tools."
        exit 1
    fi

    $MD5_EXE "$PACKAGES_CONFIG" | awk '{ print $1 }' >| "$PACKAGES_CONFIG_MD5"

    popd >/dev/null

    # Restore addins from NuGet.
    if [ -f "$ADDINS_PACKAGES_CONFIG" ]; then
        pushd "$ADDINS_DIR" >/dev/null

        mono "$NUGET_EXE" install -ExcludeVersion
        if [ $? -ne 0 ]; then
            echo "Could not restore NuGet addins."
            exit 1
        fi

        popd >/dev/null
    fi

    # Restore modules from NuGet.
    if [ -f "$MODULES_PACKAGES_CONFIG" ]; then
        pushd "$MODULES_DIR" >/dev/null

        mono "$NUGET_EXE" install -ExcludeVersion
        if [ $? -ne 0 ]; then
            echo "Could not restore NuGet modules."
            exit 1
        fi

        popd >/dev/null
    fi

    # Make sure that Cake has been installed.
    if [ ! -f "$CAKE_EXE" ]; then
        echo "Could not find Cake.exe at '$CAKE_EXE'."
        exit 1
    fi
}

ensureOthers ()
{
    # Ensure native nuget
    sudo apt-get install nuget -y
    
    # Ensure Java
    sudo apt-get install default-jre -y

    # Ensure TypeScript
    curl -o- https://raw.githubusercontent.com/creationix/nvm/v0.33.11/install.sh | bash
    sudo nvm uninstall --lts
    sudo nvm install --lts
    sudo npm install -g typescript
}

startRunning()
{
    echo "Current script root - $PSScriptRoot"
    if [ -z "$PSScriptRoot" ]; then
        echo "Modifying script root"
        PSScriptRoot=$(pwd)
    fi

    cakeScriptCount=$(ls "$PSScriptRoot" | grep ".cake" -c)
    if [ -d "$PSScriptRoot" ] && [ "$cakeScriptCount" -gt 0 ]; then
        echo "Picking parent folder because no cake file was found in root"
        PSScriptRoot=$(dirname "$PSScriptRoot")
    fi
    echo "Final script root - $PSScriptRoot"

    # get changes
    diff=()
    if [ -z "$CI_COMMIT_REF_NAME" ] || [[ "$CI_COMMIT_REF_NAME" != "origin/master" && "$CI_COMMIT_REF_NAME" != "origin/develop" ]]; then
        echo "$CI_COMMIT_REF_NAME is not a building branch... exiting"
        if [ -d "$TOOLS_DIR" ]; then
            rm -rf "$TOOLS_DIR"
        fi
        exit
    fi

    if [ ! -z $"CI_COMMIT_SHA" ]; then
        echo "Found git commits, running commit work."
        # Get the first parent, skip the first one because that is this commit
        parent=$(git rev-list --first-parent "$CI_COMMIT_SHA" -n 1 --skip 1)
        echo "parent sha - $parent"
        allDiffs=$(git diff --no-commit-id --name-only -r "$parent")
        echo "all the diffs - ${allDiffs[*]}"

        for thing in $allDiffs; do
            # Gets every file from commit and adds it to $diff
            echo "adding a diff $thing"
            diff+=("$thing")
        done
    else
        echo "Found no git commits, running $CakeTarget work."
    fi

    #(Bake-Cake, Half-Baked)
    Target="Bake-Cake"
    Configuration="Release"
    #("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")
    Verbosity="Diagnostic"

    # run if there are changes
    echo "number of changes - ${#diff[@]}"
    alreadyBuilt=() # Memoize so we don't build twice
    for change in "${diff[@]}"; do
        echo $change
        if [ ! -z "$change" ] && [ $(echo "$change" | grep "Tests" -c) -lt 1 ] && [ $(echo "$change" | grep ".sql" -c) -lt 1 ]; then
            ADDR=()
            PROJECTNAME=""
            IFS='/' read -ra ADDR <<< "$change"
            if [ "${#ADDR[@]}" -lt 2 ]; then
                continue
            fi
            PROJECTNAME="${ADDR[-2]}"
            if [ $(containsElement "$PROJECTNAME" "$alreadyBuilt") ]; then
                continue
            else
                alreadyBuilt+=$PROJECTNAME
            fi

            exec 5>&1
            runResult=$(findAndRunCakeScript "$PROJECTNAME" "$CakeTarget" "$Target" "$Configuration" "$Verbosity" "$PROJECTNAME" "$PSScriptRoot"|tee >(cat - >&5))
            echo "completed $diff"
            # findAndRunCakeScript "$PROJECTNAME" "$CakeTarget" "$Target" "$Configuration" "$Verbosity" "$PROJECTNAME" "$PSScriptRoot"
            # echo "completed $diff"
        else
            echo "skipping $diff"
        fi
    done
    
    # run if this isn't change driven
    if [ -z "$CI_COMMIT_SHA" ]; then
        ADDR=()
        IFS='/' read -ra ADDR <<< "$change"
        if [ "${#ADDR[@]}" -lt 2 ]; then
            continue
        fi
        PROJECTNAME="${ADDR[-2]}"
        # Still need the find and run cake script in this

        exec 5>&1
        runResult=$(findAndRunCakeScript "$CakeDirectory" "$CakeTarget" "$Target" "$Configuration" "$Verbosity" "$PROJECTNAME" "$PSScriptRoot"|tee >(cat - >&5))
        echo "completed $CI_COMMIT_SHA"
        # findAndRunCakeScript "$CakeDirectory" "$CakeTarget" "$Target" "$Configuration" "$Verbosity" "$PROJECTNAME" "$PSScriptRoot"
        # echo "completed $CI_COMMIT_SHA"
    fi
    echo "completed run"
}


findAndRunCakeScript ()
{
    local CAKEDIR="$1"
    local CakeTarget="$2"
    local Target="$3"
    local Configuration="$4"
    local Verbosity="$5"
    local PROJECTNAME="$6"
    local BaseDirToLink="$7"

    local Script=""
    echo "Looking for a .cake file in $CAKEDIR."
    local NewCakeDir="$BaseDirToLink"

    if [ ! -z "$AdditionalSubDir" ]; then
        echo "Adding additional subdirectory to the junction"
        NewCakeDir=$(joinPath "$NewCakeDir" "$AdditionalSubDir")
    fi

    if [ ! -d "$NewCakeDir" ]; then
        echo "Unable to find additional sub dir"
    else
        echo "Found the addtionsal sub dir"
    fi

    if [ "$AddProjectDir" = true ]; then
        echo "Adding the project directory ($PROJECTNAME) to the junction cake dir"
        NewCakeDir=$(joinPath "$NewCakeDir" "$PROJECTNAME")
    fi

    if [ ! -d "$NewCakeDir" ]; then
        echo "Unable to find project subdir"
    else
        echo "Found the project subdir"
    fi

    cakeFileCount=$(ls "$NewCakeDir" | grep ".cake" -c)
    if [ -d "$NewCakeDir" ] && [ "$cakeFileCount" -gt 0 ]; then
        if [ -n "$CakeTarget" ] || [ "$CakeTarget" =~ "Build" ]; then
            echo "Looking for Build.Cake in $NewCakeDir"

            # we are going to need this for sonarqube
            #EnsureJava
            Script=$(ls "$NewCakeDir" | grep ".cake" | head -1)
            csprojFileCount=$(ls "$NewCakeDir" | grep ".csproj" -c)
            if [ "$csprojFileCount" -le 0 ]; then
                echo "Found a cake file, but didn't find a csproj"
                return 0
            fi
        else
            echo "Can't find Cakefile for $CakeTarget in $NewCakeDir... Abandoning ship!"
            return 0
        fi
    fi

    if [ -z "$Script" ]; then
        echo "Can't find Cakefile in $NewCakeDir... Abandoning ship!"
        return 0
    fi

    Script=$(joinPath "$NewCakeDir" "$Script")

    echo "Preparing to run build script at $Script..."

    oldWorkspace="$WORKSPACE"
    export WORKSPACE="$BaseDirToLink"
    oldPath="$PATH";
    echo "old path - $oldPath"
    export PATH=$PATH:"$CAKE_DIR"
    echo "new path - $PATH"

    # Start Cake
    echo "Running build script..."
    exec mono "$CAKE_EXE" "$Script" -target="Bake-Cake" -configuration="Release" -verbosity="Diagnostic" -experimental="true"

    export WORKSPACE="$oldWorkspace"
    export PATH="$PATH"

    LASTEXITCODE="$?"

    if [ "$LASTEXITCODE" != 0 ]; then
        echo "Found error, exiting. - $LASTEXITCODE error - $allOutput"
        exit 1
    else
        echo "No error found"
    fi
}

# Start the main script

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        --) shift; CAKE_ARGUMENTS+=("$@"); break ;;
        *) CAKE_ARGUMENTS+=("$1") ;;
    esac
    shift
done

echo "workspace - $WORKSPACE"
echo "PSScriptRoot - $PSScriptRoot"
echo "Cake Target - $CakeTarget"
echo "Cake Directory - $CakeDirectory"
echo "Script Directory - $SCRIPT_DIR"

ensureCakeAndNuget

ensureOthers

startRunning

echo "finished running"

rm -rf "$TOOLS_DIR"