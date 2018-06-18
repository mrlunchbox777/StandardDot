#load "Nuget.cake"
#load "UnitTests.cake"
#load "MSBuildInfo.cake"
#load "ProjectInfo.cake"
#load "ConfigurableSettings.cake"
#load "CakeMethods.cake"

public class CakeConfig
{
    public CakeConfig(ICakeContext context, bool GetNuget = true, bool HandleUnitTests = true, bool StepUpADirectoryInConfigureSpace = false){
        this.context = context;
        ProjectInfo = new ProjectInfo(context, StepUpADirectoryInConfigureSpace);
        ConfigurableSettings = new ConfigurableSettings(context);
        CakeMethods = new CakeMethods(context);
        if (GetNuget){
            Nuget = new Nuget(context);
        }
        if (HandleUnitTests){
            UnitTests = new UnitTests(context, ProjectInfo.projectName.ToString(), ProjectInfo.projectDirectory.ToString());
        }
        MSBuildInfo = new MSBuildInfo(context);

        if (GetNuget){
            GetProjectInfo();
        }
    }

    public Nuget Nuget { get; set; }
    public UnitTests UnitTests { get; set; }
    public MSBuildInfo MSBuildInfo { get; set; }
    public ProjectInfo ProjectInfo { get; set; }
    public ConfigurableSettings ConfigurableSettings { get; set; }
    public CakeMethods CakeMethods { get; set; }
    private ICakeContext context { get; set; }

    private void GetProjectInfo()
    {
        context.Information("----------------------------------------------------------------------");    
        ProjectInfo.projectVersion = "1.0.0"; // Figure out a versioning system maybe CI_JOB_ID ?
        context.Information("-- Project Version is for: " + ProjectInfo.projectVersion); 
        context.Information("----------------------------------------------------------------------");
    }

    public string GetSearchString(string searchKeyword)
    {
        return "[CTRL+F \"" + searchKeyword + "\"]";
    }

    public void DispalyException(Exception exception, string[] potentialFixes, bool readLog, string searchKeyword = null, string exceptionTitle = null)
    {
        string tabbedWhiteSpace = "        ";
        exceptionTitle = exceptionTitle ?? "CAKE EXCEPTION";
        searchKeyword = searchKeyword ?? exception.Message;
        context.Information("----------------------------------------------------------------------");
        context.Information("\n\n\n\n");
        context.Information(tabbedWhiteSpace + "==================== " + exceptionTitle + " ====================");
        context.Information(tabbedWhiteSpace + "Exception - " + exception.Message);
        int i = 1;
        foreach (string potentialFix in potentialFixes){
            context.Information(tabbedWhiteSpace + "Recommendation " + i +": " + potentialFix);
            i++;
        }
        if (readLog)
        {
            context.Information(tabbedWhiteSpace + "The log likely contains valuable information. Please read it. " + GetSearchString(searchKeyword));
        }
        else
        {
            context.Information(tabbedWhiteSpace + "The log may or may not contain helpful debugging information. You can try " + GetSearchString(searchKeyword));
        }
        context.Information(tabbedWhiteSpace + "==================== " + exceptionTitle + " ====================");
        context.Information("\n\n\n\n");
        context.Information("----------------------------------------------------------------------");
    }
}