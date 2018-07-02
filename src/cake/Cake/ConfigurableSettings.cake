public class ConfigurableSettings
{
    public ConfigurableSettings(ICakeContext context) {
        this._context = context;
    }

    private ICakeContext _context { get; set; }

    public bool DoLocalCopyWork { get; set; }

    public bool DeleteLocalCopyDirBeforeCopy { get; set; }

    public bool DoFtpWork { get; set; }

    public string LocalCopyTargetDirectory { get; set; }

    public string SpecificWebsiteOutputDir { get; set; }

    public bool RestartIIS { get; set; }

    public bool UseRemoteServer { get; set; }

    public string RemoteIISServerName { get; set; }

    public string ApplicationPoolName { get; set; }

    public string ApplicationSiteName { get; set; }
}