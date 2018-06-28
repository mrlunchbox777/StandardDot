public class ProjectInfo
{
    public ProjectInfo(ICakeContext context, bool StepUpADirectoryInConfigureSpace = false){
        this.Context = context;
        this.StepUpADirectoryInConfigureSpace = StepUpADirectoryInConfigureSpace;
        this.IsProduction = (Context.EnvironmentVariable("CI_COMMIT_REF_NAME") ?? "develop") == "master";
    }

    private ICakeContext Context { get; set; }

    public string projectVersion = "1.0.0";

    public bool IsProduction { get; set; }

    bool StepUpADirectoryInConfigureSpace { get; set; }

    private FilePath _projectSolution;

    private FilePath _csProj;

    public string projectName {
        get
        {
            return Context.EnvironmentVariable("PROJECT_TO_BUILD")
                ?? Context.EnvironmentVariable("PROJECTNAME")
                ?? Context.Argument("project", "string")
                ?? "BAD";
        }
    }

    public DirectoryPath workspace
    {
        get
        {
            return Context.HasEnvironmentVariable("WORKSPACE")
                ? Context.MakeAbsolute(Context.Directory(Context.EnvironmentVariable("WORKSPACE")))
                : Context.MakeAbsolute(Context.Directory(StepUpADirectoryInConfigureSpace ? "../" : "./"));
        }
    }

    public DirectoryPath projectDirectory
    {
        get
        {
            return Context.DirectoryExists(Context.Directory(workspace + projectName))
                ? Context.Directory(workspace + projectName)
                : Context.Environment.WorkingDirectory;
        }
    }

    public FilePath projectSolution
    {
        get
        {
            if (_projectSolution != null){
                return _projectSolution;
            }
            Context.Information("Finding first .sln file");
            IDirectory directory = Context.FileSystem.GetDirectory(workspace);
            Context.Information("Using " + directory.Path.FullPath + ".");
            IEnumerable<IFile> files = directory.GetFiles("*.sln", SearchScope.Current);
            Context.Information("Found " + files.Count() + " files.");
            FilePath path = files.First().Path;
            Context.Information("Using " + path.FullPath + ".");
            _projectSolution = path;
            return _projectSolution;
        }
    }

    public FilePath projectFile
    {
        get
        {
            if (_csProj != null)
            {
                return _csProj;
            }
            ConvertableFilePath csproj = Context.File(projectDirectory + "/" + projectName + ".csproj");
            if (Context.FileExists(csproj.Path))
            {
                _csProj = csproj.Path;
                return _csProj;
            }
            Context.Information("Finding first .csproj file");
            IDirectory directory = Context.FileSystem.GetDirectory(projectDirectory + "/" );
            Context.Information("Using " + directory.Path.FullPath + ".");
            IEnumerable<IFile> files = directory.GetFiles("*.csproj", SearchScope.Current);
            Context.Information("Found " + files.Count() + " files.");
            FilePath path = files.First().Path;
            Context.Information("Using " + path.FullPath + ".");
            _csProj = path;
            return _csProj;
        }
    }

    public string FlattenOutputDirectory
    {
        get
        {
            return !string.IsNullOrEmpty(Context.EnvironmentVariable("MSBUILDOUTPUTDIR")) ? "\"" + Context.EnvironmentVariable("MSBUILDOUTPUTDIR") + "\"" : @"..\build\output";
        }
    }

    public SolutionParserResult parsedSolutionFile;

    public ProjectParserResult parsedProjectFile;
}