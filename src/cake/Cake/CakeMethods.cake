using System;
using FluentFTP;
using System.Linq;

public class CakeMethods
{
    public CakeMethods(ICakeContext context) {
        Context = context;
    }

    private ICakeContext Context { get; set; }

    public void CopyFolderFromProjectRootToOutput(CakeConfig cakeConfig, string currentFolderToCopy)
    {
        // make sure to set this in your cakeConfig.ConfigurableSettings
        // cakeConfig.ConfigurableSettings.specificWebsiteOutputDir = ""
        try
        {
                if (string.IsNullOrWhiteSpace(currentFolderToCopy))
                {
                    throw new CakeException("No local copy target directory variable set. Please pass - 'currentFolderToCopy' - in your build.");
                }
                string targetDir = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.specificWebsiteOutputDir + "\\" + currentFolderToCopy;
                Context.Information("--------------------------------------------------------------------------------");
                Context.Information("Starting Local Copy");
                Context.Information("--------------------------------------------------------------------------------");
                Context.CopyDirectory(cakeConfig.ProjectInfo.projectDirectory + "\\" + currentFolderToCopy, targetDir);
                Context.Information("--------------------------------------------------------------------------------");
                Context.Information("Local Copy Complete");
                Context.Information("--------------------------------------------------------------------------------");
        } catch (Exception ex) {
            cakeConfig.DispalyException(
                ex,
                new string[] {
                    "Ensure the project built correctly",
                    "Ensure no files are locked",
                    "Ensure 'currentFolderToCopy' is set in the cakeConfig.ConfigurableSettings"
                },
                true
                );
            throw;
        }
    }

    public void UploadDirectory(CakeConfig cakeConfig, FtpClient client, string localDirectory, string projectDirectory)
    {
        // make sure to set this in your cakeConfig.ConfigurableSettings
        // cakeConfig.ConfigurableSettings.ftpRemoteDir = ""
        try
        {
                if (string.IsNullOrWhiteSpace(localDirectory))
                {
                    throw new CakeException("No local directory set. Please pass - 'localDirectory' - in your build.");
                }
                Context.Information("--------------------------------------------------------------------------------");
                Context.Information("Uploading Directory - " + localDirectory);
                Context.Information("--------------------------------------------------------------------------------");
                // files first
                string[] files = System.IO.Directory.GetFiles(localDirectory);
                string remoteDir = cakeConfig.ConfigurableSettings.ftpRemoteDir + localDirectory.Replace(projectDirectory, "").Replace("\\", "/");
                client.UploadFiles(files, remoteDir, FtpExists.Overwrite, true, FtpVerify.Retry);
                // directories second
                string[] directories = System.IO.Directory.GetDirectories(localDirectory);
                foreach (string directory in directories)
                {
                    UploadDirectory(cakeConfig, client, directory, projectDirectory);
                }
                Context.Information("--------------------------------------------------------------------------------");
                Context.Information("Upload Complete - " + localDirectory);
                Context.Information("--------------------------------------------------------------------------------");
        } catch (Exception ex) {
            cakeConfig.DispalyException(
                ex,
                new string[] {
                    "Ensure the project built correctly",
                    "Ensure no files are locked",
                    "Ensure 'ftpRemoteDir' is set in the cakeConfig.ConfigurableSettings"
                },
                true
                );
            throw;
        }
    }
}