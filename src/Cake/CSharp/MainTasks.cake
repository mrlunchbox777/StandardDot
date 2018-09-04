#addin nuget:?package=Cake.VersionReader

// Load other scripts.
#load "../Main.cake"

Task("Copy-Web-Config")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Copy Web Config.");
    }
    // get the web.config
    string origWebConfigLocation = Config.ProjectInfo.ProjectDirectory + "\\Web.config.example";
    string newWebConfigLocation = Config.ProjectInfo.ProjectDirectory + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Copying - " + origWebConfigLocation + " -> " + newWebConfigLocation);
    Information("--------------------------------------------------------------------------------");
    CopyFile(origWebConfigLocation, newWebConfigLocation);
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});

Task("Copy-Web-Config-To-Output")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Copy Web Config To Output.");
    }
    // get the web.config
    string origWebConfigLocation = Config.ProjectInfo.ProjectDirectory + "\\Web.config";
    string newWebConfigLocation = Config.ProjectInfo.FlattenOutputDirectory + "\\" + Config.ConfigurableSettings.SpecificWebsiteOutputDir + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Copying - " + origWebConfigLocation + " -> " + newWebConfigLocation);
    Information("--------------------------------------------------------------------------------");
    CopyFile(origWebConfigLocation, newWebConfigLocation);
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});

Task("Remove-Web-Config")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Remove Web Config.");
    }
    // remove the web.config
    string webConfigLocation = Config.ProjectInfo.FlattenOutputDirectory + "\\" + Config.ConfigurableSettings.SpecificWebsiteOutputDir + "\\Web.config";
    Information("--------------------------------------------------------------------------------");
    Information("Deleting - " + webConfigLocation);
    Information("--------------------------------------------------------------------------------");
    DeleteFile(webConfigLocation);
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure that there is a good web.config"
        },
        true
        );
});

Task("Update-Version-From-Assembly")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "UpdatingVersionFromDll.");
    }

    string oldVersion = Config.Nuget.Version;
    string dllPath = string.IsNullOrWhiteSpace(Config.Nuget.DllDirectory)
        ? (Config.MSBuildInfo.ShouldFlatten()
            ? Config.ProjectInfo.FlattenOutputDirectory
            : Config.ProjectInfo.ProjectDirectory + "/bin/" + Config.MSBuildInfo.MsBuildConfig() + "/"
                + Config.MSBuildInfo.TargetFramework + "/")
        : Config.Nuget.DllDirectory;
    dllPath += string.IsNullOrWhiteSpace(Config.Nuget.DllName)
        ? Config.ProjectInfo.ProjectName + ".dll"
        : Config.Nuget.DllName;
    // echo out where we are getting the path

    Config.Nuget.Version = GetFullVersionNumber(dllPath);
    if (Config.Nuget.UpdateVersionWithCINumber)
    {
        Config.Nuget.Version += "-ref" + EnvironmentVariable("CI_COMMIT_SHA");
    }

    Information("--------------------------------------------------------------------------------");
    Information("Old Version - " + oldVersion + ", New Version - " + Config.Nuget.Version);
    Information("--------------------------------------------------------------------------------");
})

    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure the version is in the csproj"
        },
        true
        );
});

