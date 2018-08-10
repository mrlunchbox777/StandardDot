public class UnitTests
{
    public UnitTests(ICakeContext context, CakeConfig cakeConfig){
        this._cakeConfig = cakeConfig;
        this._context = context;
        this.JsTestPath = "";
        this.MaxQualityGateTimeoutCount = 24;
        this.QualityGateSleepLengthPerCount = 5000;
        this.XUnitOutputFile = _cakeConfig.ProjectInfo.ProjectDirectory + "/" + UnitTestProjectName + ".dll.xml";
        this.CoverageReportFilePath = _cakeConfig.ProjectInfo.ProjectDirectory + "/unitTests.xml";
        this.UnitTestProjectName = _cakeConfig.ProjectInfo.ProjectName + ".UnitTests";
        this.UnitTestDirectoryPath = _context.MakeAbsolute(_context.Directory(
                                        _cakeConfig.ProjectInfo.ProjectDirectory + "/../" + UnitTestProjectName
                                    ));
    }

    private CakeConfig _cakeConfig;

    private ICakeContext _context { get; set; }

    public string SqAnalysisUrl { get; set; }
    
    public DirectoryPath UnitTestDirectoryPath { get; set; }
   
    public string UnitTestProjectName { get; set; }

    public string CoverageReportFilePath { get; set; }

    public string XUnitOutputFile { get; set; }

    public string JsTestPath { get; set; }

    public int MaxQualityGateTimeoutCount { get; set; }

    public int QualityGateSleepLengthPerCount { get; set; }

    public string QualityGateStatus { get; set; }

    public bool QualityGateReady { get; set; }

    // DOT NET CORE

    public string TestAdapterPath { get; set; }

    public bool TestBlame { get; set; }

    public string DataCollectorName { get; set; }

    public string FilterExpression { get; set; }

    public bool NoBuildForTest { get; set; }

    public bool NoRestoreForTest { get; set; }

    public string ResultsDirectory { get; set; }

    public string SettingsFile { get; set; }

    public bool ListTests { get; set; }
}