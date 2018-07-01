public class ConfigurableSettings
{
    public ConfigurableSettings(ICakeContext context) {
        this.Context = context;
    }

    private ICakeContext Context { get; set; }

    public bool doLocalCopyWork { get; set; }

    public bool deleteLocalCopyDirBeforeCopy { get; set; }

    public bool doFtpWork { get; set; }

    public string localCopyTargetDirectory { get; set; }

    public string specificWebsiteOutputDir { get; set; }

    public string ftpHost { get; set; }

    public string ftpRemoteDir { get; set; }
    
    public string ftpUsername { get; set; }

    public string ftpSecurePasswordLocation { get; set; }

    public byte[] ftpAesKey => System.Convert.FromBase64String(Context.EnvironmentVariable("FTPAesKey"));

    public int ftpDeleteRetryAttempts { get; set; }

    public int ftpUploadRetryAttempts { get; set; }

    public int ftpSocketPollInterval { get; set; }

    public int ftpConnectTimeout { get; set; }

    public int ftpReadTimeout { get; set; }

    public int ftpDataConnectionConnectTimeout { get; set; }

    public int ftpDataConnectionReadTimeout { get; set; }

    public string slackChannel { get; set; }

    public bool postSlackStartAndStop { get; set; }

    public bool postSlackSteps { get; set; }

    public bool postSlackErrors { get; set; }

    public string airbrakeProjectId { get; set; }

    public string airbrakeUserName { get; set; }

    public string airbrakeEmail { get; set; }

    public bool restartIIS { get; set; }

    public bool useRemoteServer { get; set; }

    public string remoteIISServerName { get; set; }

    public string applicationPoolName { get; set; }

    public string applicationSiteName { get; set; }
}