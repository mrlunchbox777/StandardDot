#addin "Cake.Slack"

using System;
using FluentFTP;
using System.Linq;
using Cake.Slack;

public class CakeMethods
{
    public CakeMethods(ICakeContext context) {
        _context = context;
    }

    private ICakeContext _context { get; set; }

    public void SendSlackNotification(CakeConfig cakeConfig, string message, string channel = null)
    {
        try {
            var slackhookuri = cakeConfig.Slack.SlackHookUri;
            var postMessageResult = _context.Slack().Chat.PostMessage(
                        channel: channel ?? cakeConfig.Slack.SlackChannel,
                        text: message,
                        messageSettings: new SlackChatMessageSettings { IncomingWebHookUrl = slackhookuri }
                );

            if (postMessageResult.Ok)
            {
                _context.Information("Message successfully sent");
            }
            else
            {
                _context.Error("Failed to send message: {0}", postMessageResult.Error);
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
        // cakeConfig.ConfigurableSettings.SpecificWebsiteOutputDir = ""
        try
        {
                if (string.IsNullOrWhiteSpace(currentFolderToCopy))
                {
                    throw new CakeException("No local copy target directory variable set. Please pass - 'currentFolderToCopy' - in your build.");
                }
                string targetDir = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.SpecificWebsiteOutputDir + "\\" + currentFolderToCopy;
                if (!string.IsNullOrWhiteSpace(additionalPath))
                {
                    targetDir += additionalPath;
                }
                _context.Information("--------------------------------------------------------------------------------");
                _context.Information("Starting Local Copy - " + currentFolderToCopy);
                _context.Information("--------------------------------------------------------------------------------");
                _context.CopyDirectory(cakeConfig.ProjectInfo.ProjectDirectory + "\\" + currentFolderToCopy, targetDir);
                _context.Information("--------------------------------------------------------------------------------");
                _context.Information("Local Copy Complete");
                _context.Information("--------------------------------------------------------------------------------");
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
        // cakeConfig.ConfigurableSettings.SpecificWebsiteOutputDir = ""
        try
        {
                if (string.IsNullOrWhiteSpace(currentFileToCopy))
                {
                    throw new CakeException("No local copy target directory variable set. Please pass - 'currentFolderToCopy' - in your build.");
                }
                string targetFile = cakeConfig.ProjectInfo.FlattenOutputDirectory + "\\" + cakeConfig.ConfigurableSettings.SpecificWebsiteOutputDir + "\\" + currentFileToCopy;
                _context.Information("--------------------------------------------------------------------------------");
                _context.Information("Starting Local Copy - " + currentFileToCopy);
                _context.Information("--------------------------------------------------------------------------------");
                _context.CopyFile(cakeConfig.ProjectInfo.ProjectDirectory + "\\" + currentFileToCopy, targetFile);
                _context.Information("--------------------------------------------------------------------------------");
                _context.Information("Local Copy Complete");
                _context.Information("--------------------------------------------------------------------------------");
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
        // cakeConfig.FtpHelper.RemoteDir = ""
        try
        {
                if (string.IsNullOrWhiteSpace(localDirectory))
                {
                    throw new CakeException("No local directory set. Please pass - 'localDirectory' - in your build.");
                }
                _context.Information("--------------------------------------------------------------------------------");
                _context.Information("Uploading Directory - " + localDirectory);
                _context.Information("--------------------------------------------------------------------------------");
                // files first
                string[] files = System.IO.Directory.GetFiles(localDirectory);
                string remoteDir = cakeConfig.FtpHelper.RemoteDir + localDirectory.Replace(projectDirectory, "").Replace("\\", "/");
                client.UploadFiles(files, remoteDir, FtpExists.Overwrite, true, FtpVerify.Retry);
                // directories second
                string[] directories = System.IO.Directory.GetDirectories(localDirectory);
                foreach (string directory in directories)
                {
                    UploadDirectory(cakeConfig, client, directory, projectDirectory);
                }
                _context.Information("--------------------------------------------------------------------------------");
                _context.Information("Upload Complete - " + localDirectory);
                _context.Information("--------------------------------------------------------------------------------");
        } catch (Exception ex) {
            cakeConfig.DispalyException(
                ex,
                new string[] {
                    "Ensure the project built correctly",
                    "Ensure no files are locked",
                    "Ensure 'RemoteDir' is set in the Config.FtpHelper"
                },
                true
                );
            throw;
        }
    }
}