public class Nuget
{
    public Nuget(ICakeContext context, CakeConfig cakeConfig){
        this._context = context;
        this._config = cakeConfig;

        string description = ("repo - "
                                    + (!string.IsNullOrWhiteSpace(this._context.EnvironmentVariable("CI_REPOSITORY_URL"))
                                        ? this._context.EnvironmentVariable("CI_REPOSITORY_URL")
                                        : "Unknown")
                                    + ", commit - "
                                    + (!string.IsNullOrWhiteSpace(this._context.EnvironmentVariable("CI_COMMIT_SHA"))
                                        ? this._context.EnvironmentVariable("CI_COMMIT_SHA")
                                        : "Unknown")
                                    ).ToString();
        this.Summary = string.IsNullOrWhiteSpace(this.Summary)
            ? description
            : this.Summary;
        this.Description = string.IsNullOrWhiteSpace(this.Description)
            ? description
            : this.Description;
        this.Server = null;
        this.ServerFeed = "https://api.nuget.org/v3/index.json";

        this.PackPath = _config.ProjectInfo.ProjectDirectory + "/" + _config.ProjectInfo.ProjectName + ".nuspec";

        this.Id = _config.ProjectInfo.ProjectName;
        this.Version = "0.0.0.1";
        this.Title = this.Id;
        this.Authors = new List<string>(){ "Standard Dot" };
        this.Owners = new List<string>(){ "Standard Dot" };
        this.ProjectUrl = string.IsNullOrWhiteSpace(_context.EnvironmentVariable("CI_REPOSITORY_URL"))
            ? null
            : new Uri(_context.EnvironmentVariable("CI_REPOSITORY_URL"));
        this.IconUrl = new Uri(
            "https://github.com/mrlunchbox777/StandardDot/blob/master/defaultNugetIcon.png");
        this.LicenseUrl = new Uri("https://github.com/IQAndreas/markdown-licenses/blob/master/mit.md");
        this.Copyright = "Standard Dot " + DateTime.UtcNow.Year.ToString();
        this.ReleaseNotes = new List<string>(){ "General Updates" };
        this.Tags = new List<string>(){ "Build" };
        this.RequireLicenseAcceptance = false;
        this.Symbols = false;
        this.NoPackageAnalysis = false;
        this.Files = null;
        this.BasePath = _config.ProjectInfo.Workspace + "/bin/" + _config.MSBuildInfo.MsBuildConfig() + "/";
        this.OutputDirectory = _config.ProjectInfo.Workspace;

        this.IncludeReferencedProjects = true;
    }

    private ICakeContext _context { get; set; }

    private CakeConfig _config { get; set; }

    public string ApiKey { get { return _context.EnvironmentVariable("NUGET_APIKEY"); } }

    public DirectoryPath PackagesDirectory { get { return _context.MakeAbsolute(_context.Directory("../packages")); } }

    public string Server { get; set; }

    public string ServerFeed { get; set; }
    
    public string PackPath { get; set; }

    public string PackDirectory { get; set; }

    public bool CreateNugetPackage { get; set; }

    // Nuspec Settings
    public string Id { get; set; }

    public string Version { get; set; }

    public string Title { get; set; }

    public IEnumerable<string> Authors { get; set; }

    public IEnumerable<string> Owners { get; set; }

    public string Description { get; set; }

    public string Summary { get; set; }

    public Uri ProjectUrl { get; set; }

    public Uri IconUrl { get; set; }

    public Uri LicenseUrl { get; set; }

    public string Copyright { get; set; }

    public IEnumerable<string> ReleaseNotes { get; set; }

    public IEnumerable<string> Tags { get; set; }

    public bool RequireLicenseAcceptance { get; set; }

    public bool Symbols { get; set; }

    public bool NoPackageAnalysis { get; set; }

    public IEnumerable<NuSpecContent> Files { get; set; }

    public string BasePath { get; set; }

    public DirectoryPath OutputDirectory { get; set; }

    public bool IncludeReferencedProjects { get; set; }

    // DOT NET CORE

    public bool UpdateVersionWithCINumber { get; set; }

    public string DllDirectory { get; set; }

    public string DllName { get; set; }

    // PACK

    public bool Force { get; set; }

    public bool IncludeSource { get; set; }

    public bool BuildForPack { get; set; }

    public bool IgnoreDependencies { get; set; }

    public bool NoRestore { get; set; }

    public string RunTimeVersion { get; set; }

    public bool Servicable { get; set; }

    public string VersionSuffix { get; set; }

    public string VerbosityLevel { get; set; }

    // PUSH

    public bool DisableBuffering { get; set; }

    public bool ForceEnglishOutput { get; set; }

    public bool NoPushSymbols { get; set; }

    public bool NoServiceEndpoint { get; set; }

    public string SymbolApiKey { get; set; }

    public string SymbolSource { get; set; }

    public int Timeout { get; set; }
}