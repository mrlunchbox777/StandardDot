public class UnitTests
{
    public UnitTests(ICakeContext context, CakeConfig cakeConfig){
        this._cakeConfig = cakeConfig;
        this.Context = context;
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
}