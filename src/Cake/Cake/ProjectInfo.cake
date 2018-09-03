public class ProjectInfo
{
    public ProjectInfo(ICakeContext context, bool stepUpADirectoryInConfigureSpace = false){
        this._context = context;
        this._stepUpADirectoryInConfigureSpace = stepUpADirectoryInConfigureSpace;
        this.IsProduction = (_context.EnvironmentVariable("CI_COMMIT_REF_NAME") ?? "develop").Contains("master");
        this.IsBeta = (_context.EnvironmentVariable("CI_COMMIT_REF_NAME") ?? "develop").Contains("beta");
        this.IsQa = (_context.EnvironmentVariable("CI_COMMIT_REF_NAME") ?? "develop").Contains("qa");
        this.IsIntegration = (_context.EnvironmentVariable("CI_COMMIT_REF_NAME") ?? "develop").Contains("integration");
        this.IsDevelopment = (_context.EnvironmentVariable("CI_COMMIT_REF_NAME") ?? "develop").Contains("develop");
        this.IsLocal = !(this.IsProduction || this.IsBeta || this.IsQa
            || this.IsIntegration || this.IsDevelopment);
    }

    private bool _stepUpADirectoryInConfigureSpace { get; set; }

    private FilePath _projectSolution;

    private FilePath _csProj;

    private ICakeContext _context { get; set; }

    public bool IsLocal { get; set; }

    public bool IsDevelopment { get; set; }

    public bool IsIntegration { get; set; }

    public bool IsQa { get; set; }

    public bool IsBeta { get; set; }

    public bool IsProduction { get; set; }

    public string EnvironmentName
    {
        get
        {
            string environmentName = (_context.EnvironmentVariable("CI_COMMIT_REF_NAME").Split('/')).Last();
            if (IsProduction)
            {
                environmentName = "production";
            }
            if (IsLocal)
            {
                environmentName = "local";
            } 
            return environmentName;
        }
    }

    private string _projectName = null;

    public string ProjectName {
        get
        {
            return _projectName
                ?? _context.EnvironmentVariable("PROJECT_TO_BUILD")
                ?? _context.EnvironmentVariable("PROJECTNAME")
                ?? _context.Argument("project", "string")
                ?? "BAD";
        }
    }

    public DirectoryPath Workspace
    {
        get
        {
            return _context.HasEnvironmentVariable("WORKSPACE")
                ? _context.MakeAbsolute(_context.Directory(_context.EnvironmentVariable("WORKSPACE")))
                : _context.MakeAbsolute(_context.Directory(_stepUpADirectoryInConfigureSpace ? "../" : "./"));
        }
    }

    public DirectoryPath ProjectDirectory
    {
        get
        {
            DirectoryPath directory = _context.DirectoryExists(_context.Directory(Workspace + ProjectName))
                ? _context.Directory(Workspace + ProjectName)
                : _context.Environment.WorkingDirectory;
            
            _projectName = directory == null
                ? _projectName
                : directory.ToString().Split('/').LastOrDefault() ?? _projectName;
            
            return directory;
        }
    }

    public FilePath ProjectSolution
    {
        get
        {
            if (_projectSolution != null){
                return _projectSolution;
            }
            _context.Information("Finding first .sln file");
            IDirectory directory = _context.FileSystem.GetDirectory(Workspace);
            _context.Information("Using " + directory.Path.FullPath + ".");
            IEnumerable<IFile> files = directory.GetFiles("*.sln", SearchScope.Current);
            _context.Information("Found " + files.Count() + " files.");
            FilePath path = files.First().Path;
            _context.Information("Using " + path.FullPath + ".");
            _projectSolution = path;
            return _projectSolution;
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
            ConvertableFilePath csproj = _context.File(ProjectDirectory + "/" + ProjectName + ".csproj");
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
            IDirectory directory = _context.FileSystem.GetDirectory(ProjectDirectory + "/" );
            _context.Information("Using " + directory.Path.FullPath + ".");
            IEnumerable<IFile> files = directory.GetFiles("*.csproj", SearchScope.Current);
            _context.Information("Found " + files.Count() + " files.");
            FilePath path = files.First().Path;
            _context.Information("Using " + path.FullPath + ".");
            _csProj = path;
            return _csProj;
        }
    }

    public string FlattenOutputDirectory
    {
        get
        {
            return !string.IsNullOrEmpty(_context.EnvironmentVariable("MSBUILDOUTPUTDIR"))
                ? "\"" + _context.EnvironmentVariable("MSBUILDOUTPUTDIR") + "\""
                : @"..\build\output";
        }
    }
}