// Load other scripts.
#load "../Main.cake"

Task("Sass-Compile")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Sass Compile.");
    }
    Information("--------------------------------------------------------------------------------");
    Information("Compiling Sass - ");
    Information("--------------------------------------------------------------------------------");
    try {
        StartProcess("..\\scss_compiler.bat");
    } catch (Exception ex) {
        Information("Got and ignoring error - " + ex.Message + ".\r\nStack trace -\r\n" + ex.StackTrace);
    }
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