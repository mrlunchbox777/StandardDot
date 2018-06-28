public class UnitTests
{
    public UnitTests(ICakeContext context, string ProjectName, string ProjectDirectory){
        this._projectName = ProjectName;
        this._projectDirectory = ProjectDirectory;
        this.Context = context;
    }
    private string _projectName;
    public string projectName {
        get { return _projectName; }
        set { this._projectName = value; }
    }
    private string _projectDirectory;
    public string projectDirectory {
        get { return _projectDirectory; }
        set { this._projectDirectory = value; }
    }
    private string _sqAnalysisUrl = string.Empty;
    public string sqAnalysisUrl {
        get {return _sqAnalysisUrl;}
        set {this._sqAnalysisUrl = value;}
    }

    private ICakeContext Context { get; set; }

    public string unitTestProjectName { get { return projectName + ".UnitTests"; } }
    public DirectoryPath unitTestDirectoryPath { get { return Context.MakeAbsolute(Context.Directory(projectDirectory + "/../" + unitTestProjectName)); } }
    public string coverageReportFilePath { get { return projectDirectory + "/unitTests.xml"; } }
    public string xUnitOutputFile { get { return projectDirectory + "/" + unitTestProjectName + ".dll.xml"; } }
    public string jsTestPath = "";
    public int maxQualityGateTimeoutCount { get { return 24; } }
    public int QualityGateSleepLengthPerCount { get { return 5000; } }
    public string qualityGateStatus { get; set; }
    public bool qualityGateReady { get; set; }
}