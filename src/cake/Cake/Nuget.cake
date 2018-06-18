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
        this.nugetSummary = this.nugetSummary == "Unknown" ? description : this.nugetSummary;
        this.nugetDescription = this.nugetDescription == "Unknown" ? description : this.nugetDescription;
    }

    private ICakeContext Context { get; set; }
    private CakeConfig Config { get; set; }
    public string nugetServerURL = this.Config.ConfigurableSettings.NugetServer ?? "nuget.org";
    public string nugetAPIKey { get { return Context.EnvironmentVariable("NUGET_APIKEY"); } }
    public DirectoryPath packagesDirectory { get { return Context.MakeAbsolute(Context.Directory("../packages")); } }

    // Nuspec Settings
    public string nugetTitle = this.Config.ConfigurableSettings.NugetTitle ?? "A Standard Dot Package";
    public List<string> nugetAuthors = new List<string>(){ this.Config.ConfigurableSettings.NugetAuthor ?? "Standard Dot" };
    public List<string> nugetOwners = new List<string>(){ this.Config.ConfigurableSettings.NugetOwner ?? "Standard Dot" };
    public string nugetDescription = this.Config.ConfigurableSettings.NugetDescription ?? "Unknown";
    public string nugetSummary = this.Config.ConfigurableSettings.NugetSummary ?? "Unknown";
    public bool nugetIncludeReferencedProjects = true;
    public Uri nugetIconUrl = new Uri(this.Config.ConfigurableSettings.NugetIconUrl ??
        "https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/NuGet_project_logo.svg/220px-NuGet_project_logo.svg.png");
}