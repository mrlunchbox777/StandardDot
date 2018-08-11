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
        context.Information("Creating the configuration"); 
        this._context = context;
        _context.Information("Added the context"); 

        ProjectInfo = new ProjectInfo(context, stepUpADirectoryInConfigureSpace);
        _context.Information("Added the default project info"); 

        ConfigurableSettings = new ConfigurableSettings(context);
        _context.Information("Added the default configurable settings"); 

        CakeMethods = new CakeMethods(context);
        _context.Information("Added the cake methods"); 

        Slack = new CustomSlack(context);
        _context.Information("Added the default slack configuration"); 

        Airbrake = new Airbrake(context);
        _context.Information("Added the default Airbrake configuration"); 
        
        FtpHelper = new FtpHelper(context);
        _context.Information("Added the default FtpHelper configuration"); 

        MSBuildInfo = new MSBuildInfo(context);
        _context.Information("Added the default MSBuild Configuration"); 

        if (getNuget){
            Nuget = new Nuget(context, this);
            _context.Information("Added the default Nuget configuration"); 
        }

        if (handleUnitTests){
            UnitTests = new UnitTests(context, this);
            _context.Information("Added the default Unit Test configuration"); 
        }

        if (getNuget){
            GetProjectInfo();
            _context.Information("Got the Project Info based on Nuget"); 
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