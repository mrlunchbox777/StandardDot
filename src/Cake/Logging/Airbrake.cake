// Load other scripts.
#load "../Main.cake"

//////////////////////////////////////////////////////////////////////
// Airbrake Deploy
//////////////////////////////////////////////////////////////////////

Task("Send-An-Airbrake-Deploy")
    .WithCriteria(() => DirectoryExists(Config.Airbrake.DoAirbrakeDeploy))
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Send An Airbrake Deploy.");
    }

    Information("--------------------------------------------------------------------------------");
    Information("Starting Send An Airbrake Deploy");
    Information("--------------------------------------------------------------------------------");
    var settings = new HttpSettings
    {
        Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        },
    };
    settings.SetRequestBody("{\"environment\":\"" + Config.ProjectInfo.EnvironmentName + "\",\"username\":\""
        + Config.Airbrake.UserName + "\",\"email\":\"" + Config.Airbrake.Email + "\","
        + "\"repository\":\"" + EnvironmentVariable("CI_REPOSITORY_URL") + "/" + EnvironmentVariable("CI_COMMIT_REF_NAME") + "\",\"revision\":\""
        + EnvironmentVariable("CI_COMMIT_SHA") + "\",\"version\":\"v2.0\"}");

    string responseBody = HttpPost("https://airbrake.io/api/v4/projects/" + Config.Airbrake.ProjectId
        + "/deploys?key=" + Config.Airbrake.ProjectKey, settings);
    Information("--------------------------------------------------------------------------------");
    Information("Send An Airbrake Deploy Complete");
    Information("--------------------------------------------------------------------------------");
})
    .ReportError(exception =>
{
    Config.DispalyException(
        exception,
        new string[] {
            "Ensure the project built correctly",
            "Ensure no files are locked",
            "Ensure 'Config.ConfigurableSettings.LocalCopyTargetDirectory' is set in the Config.ConfigurableSettings"
        },
        true
        );
});