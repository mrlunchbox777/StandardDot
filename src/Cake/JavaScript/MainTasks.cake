// Load other scripts.
#load "../Main.cake"

Task("Type-Script-Compile")
    .Does(() =>
{
    if (Config.Slack.PostSlackSteps)
    {
        Config.CakeMethods.SendSlackNotification(Config, "Starting Type Script Compile.");
    }
    Information("--------------------------------------------------------------------------------");
    Information("Compiling TSC - ");
    Information("--------------------------------------------------------------------------------");
    try {
        //StartPowershellScript("C:\\Windows\\System32\\config\\systemprofile\\AppData\\Roaming\\npm\\tsc.cmd");
        StartProcess("C:\\Windows\\System32\\config\\systemprofile\\AppData\\Roaming\\npm\\tsc.cmd");
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