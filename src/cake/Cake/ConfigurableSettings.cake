public class ConfigurableSettings
{
    public ConfigurableSettings(ICakeContext context) {
        this.Context = context;
    }

    private ICakeContext Context { get; set; }

    public string localCopyTargetDirectory { get; set; }

    public string specificWebsiteOutputDir { get; set; }

    public string ftpHost { get; set; }

    public string ftpRemoteDir { get; set; }
    
    public string ftpUsername { get; set; }

    public string ftpSecurePasswordLocation { get; set; }

    public string NugetAuthor { get; set; }

    public string NugetOwner { get; set; }

    public string NugetServer { get; set; }

    public string NugetTitle { get; set; }

    public string NugetDescription { get; set; }

    public string NugetSummary { get; set; }

    public string NugetIconUrl { get; set; }
}