public class UnitTests
{
    public UnitTests(ICakeContext context, CakeConfig cakeConfig){
        this._cakeConfig = cakeConfig;
        this._context = context;
        this.JsTestPath = "";
        this.MaxQualityGateTimeoutCount = 24;
        this.QualityGateSleepLengthPerCount = 5000;
    }

    private CakeConfig _cakeConfig;

    private FilePath _csProj;

    private ICakeContext _context { get; set; }

    public string SqAnalysisUrl { get; set; }
    
    public DirectoryPath UnitTestDirectoryPath
    {
        get
        {
            return _context.MakeAbsolute(_context.Directory(
                                        _cakeConfig.ProjectInfo.ProjectDirectory + "/../" + UnitTestProjectName
                                    ));
        }
    }
   
    public string UnitTestProjectName
    {
        get
        {
            return _cakeConfig.ProjectInfo.ProjectName + "UnitTests";
        }
    }

    public string CoverageReportFilePath
    {
        get
        {
            return _cakeConfig.ProjectInfo.ProjectDirectory + "/unitTests.xml";
        }
    }

    public string XUnitOutputFile
    {
        get
        {
            return  _cakeConfig.ProjectInfo.ProjectDirectory + "/" + UnitTestProjectName + ".dll.xml";
        }
    }

    public FilePath ProjectFile
    {
        get
        {
            if (_csProj != null)
            {
                return _csProj;
            }
            ConvertableFilePath csproj = _context.File(UnitTestDirectoryPath + "/" + UnitTestProjectName + ".csproj");
            if (_context.FileExists(csproj.Path))
            {
                _csProj = csproj.Path;
                return _csProj;
            }
            else
            {
                _context.Information("Could find project - " + csproj.Path);
            }
            _context.Information("Finding first .csproj file");
            IDirectory directory = _context.FileSystem.GetDirectory(UnitTestDirectoryPath + "/" );
            _context.Information("Using " + directory.Path.FullPath + ".");
            IEnumerable<IFile> files = directory.GetFiles("*.csproj", SearchScope.Current);
            _context.Information("Found " + files.Count() + " files.");
            FilePath path = files.First().Path;
            _context.Information("Using " + path.FullPath + ".");
            _csProj = path;
            return _csProj;
        }
    }

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

    public string TargetFramework { get; set; }
}