#addin "Cake.Slack"

using System;
using FluentFTP;
using System.Linq;
using Cake.Slack;

public class CakeMethods
{
    public CakeMethods(ICakeContext context) {
        Context = context;
    }

    private ICakeContext Context { get; set; }

    public void SendSlackNotification(CakeConfig cakeConfig, string message, string channel = null)
    {
        try {
            var slackhookuri = cakeConfig.Slack.SlackHookUri;
            var postMessageResult = Context.Slack().Chat.PostMessage(
                        channel: channel ?? cakeConfig.ConfigurableSettings.slackChannel,
                        text: message,
                        messageSettings: new SlackChatMessageSettings { IncomingWebHookUrl = slackhookuri }
                );

            if (postMessageResult.Ok)
            {
                Context.Information("Message successfully sent");
            }
            else
            {
                Context.Error("Failed to send message: {0}", postMessageResult.Error);
            }
        } catch (Exception ex) {
            cakeConfig.DispalyException(
                ex,
                new string[] {
                    "Failed to send a slack message, is the hook uri correct?",
                    "Failed to send a slack message, is slack down?"
                },
                true
                );
            throw;
        }
    }

    public void CopyFolderFromProjectRootToOutput(CakeConfig cakeConfig, string currentFolderToCopy, string additionalPath = null)
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
                if (!string.IsNullOrWhiteSpace(additionalPath))
                {
                    targetDir += additionalPath;
                }
                Context.Information("--------------------------------------------------------------------------------");
                Context.Information("Starting Local Copy - " + currentFolderToCopy);
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

    public void CopyFileFromProjectRootToOutput(CakeConfig cakeConfig, string currentFileToCopy)
    {
        // make sure to set this in your cakeConfig.ConfigurableSettings
        // cakeConfig.ConfigurableSettings.specificWebsiteOutputDir = ""
        try
        {
                if (string.IsNullOrWhiteSpace(currentFileToCopy))
                {
                    throw new CakeException("No local copy target directory variable set. Please pass - 'currentFolderToCopy' - in your build.");
                }
                string targetFile = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.specificWebsiteOutputDir + "\\" + currentFileToCopy;
                Context.Information("--------------------------------------------------------------------------------");
                Context.Information("Starting Local Copy - " + currentFileToCopy);
                Context.Information("--------------------------------------------------------------------------------");
                Context.CopyFile(cakeConfig.ProjectInfo.projectDirectory + "\\" + currentFileToCopy, targetFile);
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