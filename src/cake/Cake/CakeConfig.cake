#load "Nuget.cake"
#load "UnitTests.cake"
#load "MSBuildInfo.cake"
#load "ProjectInfo.cake"
#load "ConfigurableSettings.cake"
#load "CakeMethods.cake"
#load "FtpHelper.cake"
#load "Slack.cake"
#load "Airbrake.cake"

public class CakeConfig
{
    public CakeConfig(ICakeContext context, bool getNuget = true, bool handleUnitTests = true, bool stepUpADirectoryInConfigureSpace = false){
        this._context = context;
        ProjectInfo = new ProjectInfo(context, stepUpADirectoryInConfigureSpace);
        ConfigurableSettings = new ConfigurableSettings(context);
        CakeMethods = new CakeMethods(context);
        Slack = new CustomSlack(context);
        Airbrake = new Airbrake(context);
        FtpHelper = new FtpHelper();
        if (getNuget){
            Nuget = new Nuget(context);
        }
        if (handleUnitTests){
            UnitTests = new UnitTests(context, ProjectInfo.ProjectName.ToString(), ProjectInfo.ProjectDirectory.ToString());
        }
        MSBuildInfo = new MSBuildInfo(context);

        if (getNuget){
            GetProjectInfo();
        }
    }

    public Nuget Nuget { get; set; }

    public UnitTests UnitTests { get; set; }

    public MSBuildInfo MSBuildInfo { get; set; }
    
    public ProjectInfo ProjectInfo { get; set; }

    public ConfigurableSettings ConfigurableSettings { get; set; }

    public FtpHelper FtpHelper { get; set; }

    public CakeMethods CakeMethods { get; set; }

    public CustomSlack Slack { get; set; }

    public Airbrake Airbrake { get; set; }

    private ICakeContext _context { get; set; }

    private void GetProjectInfo()
    {
        _context.Information("----------------------------------------------------------------------");    
        _context.Information("-- Project Version is for: " + Nuget.Version); 
        _context.Information("----------------------------------------------------------------------");
    }

    public string GetSearchString(string searchKeyword)
    {
        return "[CTRL+F \"" + searchKeyword + "\"]";
    }

    public void DispalyException(Exception exception, string[] potentialFixes, bool readLog, string searchKeyword = null, string exceptionTitle = null)
    {
        if (Slack.PostSlackErrors)
        {
            CakeMethods.SendSlackNotification(this, "ERROR DURING DEPLOY - Exception - " + exception.Message);
        }

        string tabbedWhiteSpace = "        ";
        exceptionTitle = exceptionTitle ?? "CAKE EXCEPTION";
        searchKeyword = searchKeyword ?? exception.Message;
        _context.Information("----------------------------------------------------------------------");
        _context.Information("\n\n\n\n");
        _context.Information(tabbedWhiteSpace + "==================== " + exceptionTitle + " ====================");
        _context.Information(tabbedWhiteSpace + "Exception - " + exception.Message);
        int i = 1;
        foreach (string potentialFix in potentialFixes){
            _context.Information(tabbedWhiteSpace + "Recommendation " + i +": " + potentialFix);
            i++;
        }
        if (readLog)
        {
            _context.Information(tabbedWhiteSpace + "The log likely contains valuable information. Please read it. " + GetSearchString(searchKeyword));
        }
        else
        {
            _context.Information(tabbedWhiteSpace + "The log may or may not contain helpful debugging information. You can try " + GetSearchString(searchKeyword));
        }
        _context.Information(tabbedWhiteSpace + "==================== " + exceptionTitle + " ====================");
        _context.Information("\n\n\n\n");
        _context.Information("----------------------------------------------------------------------");
    }
}