public class Nuget
{
    public Nuget(ICakeContext context, CakeConfig cakeConfig){
        this.Context = context;
        this.Config = cakeConfig;

        string description = ("repo - "
                                    + (!string.IsNullOrEmpty(this.Context.EnvironmentVariable("CI_REPOSITORY_URL")) ? this.Context.EnvironmentVariable("CI_REPOSITORY_URL") : "Unknown")
                                    + ", commit - "
                                    + (!string.IsNullOrEmpty(this.Context.EnvironmentVariable("CI_COMMIT_SHA")) ? this.Context.EnvironmentVariable("CI_COMMIT_SHA") : "Unknown")
                                    ).ToString();
        this.nugetSummary = string.IsNullOrWhiteSpace(this.nugetSummary) ? description : this.nugetSummary;
        this.nugetDescription = string.IsNullOrWhiteSpace(this.nugetDescription) ? description : this.nugetDescription;
        this.nugetServer = "nuget.org";
        this.nugetServerFeed = "nuget.org/nugetfeed";

        this.NugetPackPath = Context.ProjectInfo.projectDirectory + "/" + Context.ProjectInfo.projectName + ".nuspec";

        this.Id = Context.ProjectInfo.projectName;
        this.Version = "1.0.0.0";
        this.nugetTitle = this.Id;
        this.nugetAuthors = new List<string>(){ "Standard Dot" };
        this.nugetOwners = new List<string>(){ "Standard Dot" };
        this.nugetProjectUrl = string.IsNullOrWhiteSpace(EnvironmentVariable("CI_REPOSITORY_URL")) ? "" : new Uri(EnvironmentVariable("CI_REPOSITORY_URL"));
        this.nugetIconUrl = new Uri(
            "https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/NuGet_project_logo.svg/220px-NuGet_project_logo.svg.png");
        this.licenseUrl = new Uri("https://github.com/IQAndreas/markdown-licenses/blob/master/mit.md");
        this.copyright = "Standard Dot " + DateTime.Now.Year.ToString();
        this.releaseNotes = new List<string>(){ "General Updates" };
        this.tags = new List<string>(){ "Build" };
        this.requireLicenseAcceptance = false;
        this.symbols = false;
        this.noNugetPackageAnalysis = false;
        this.files = null;
        this.basePath = Config.ProjectInfo.workspace + "/bin/" + Config.MSBuildInfo.msbuildConfig() + "/";
        this.outputDirectory = Config.ProjectInfo.workspace;

        this.nugetIncludeReferencedProjects = true;
    }

    private ICakeContext Context { get; set; }
    private CakeConfig Config { get; set; }
    public string nugetAPIKey { get { return Context.EnvironmentVariable("NUGET_APIKEY"); } }
    public DirectoryPath packagesDirectory { get { return Context.MakeAbsolute(Context.Directory("../packages")); } }
    public string nugetServer { get; set; }
    public string nugetServerFeed { get; set; }
    public string NugetPackPath { get; set; }

    // Nuspec Settings
    public string Id { get; set; }

    public string Version { get; set; }

    public string nugetTitle { get; set; }

    public IEnumerable<string> nugetAuthors { get; set; }

    public IEnumerable<string> nugetOwners { get; set; }

    public string nugetDescription { get; set; }

    public string nugetSummary { get; set; }

    public Uri nugetProjectUrl { get; set; }

    public Uri nugetIconUrl { get; set; }

    public Uri licenseUrl { get; set; }

    public string copyright { get; set; }

    public IEnumerable<string> releaseNotes { get; set; }

    public IEnumerable<string> tags { get; set; }

    public bool requireLicenseAcceptance { get; set; }

    public bool symbols { get; set; }

    public bool noPackageAnalysis { get; set; }

    public IEnumerable<NuSpecContent> files { get; set; }

    public string basePath { get; set; }

    public string outputDirectory { get; set; }

    public bool nugetIncludeReferencedProjects { get; set; }
}