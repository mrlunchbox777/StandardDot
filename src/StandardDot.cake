#load "./Cake/CSharp/MainDotNetCoreTasks.cake"

Task("Common-Setup-Environment")
	.Does(() =>
	{
		// setting the build directory
		Config.ConfigurableSettings.LocalCopyTargetDirectory = @"";
		Config.ConfigurableSettings.SpecificWebsiteOutputDir = @"";

		Config.ConfigurableSettings.DoLocalCopyWork = false;
		Config.ConfigurableSettings.DoFtpWork = false;

		Config.Slack.SlackChannel = "";
		Config.Slack.PostSlackStartAndStop = false;
		Config.Slack.PostSlackSteps = false;
		Config.Slack.PostSlackErrors = false;

		Config.ConfigurableSettings.DeleteLocalCopyDirBeforeCopy = false;

		Config.Nuget.CreateNugetPackage = Config.ProjectInfo.IsProduction;
		Config.Nuget.BuildForPack = true;
		Config.Nuget.Server = "https://www.nuget.org/";
		Config.Nuget.Symbols = true;
		Config.Nuget.IncludeSource = true;
		// Config.Nuget.Version = null;

		Config.MSBuildInfo.TargetFramework = "netstandard2.0";
		Config.MSBuildInfo.NoIncremental = true;

		Config.UnitTests.TestBlame = true;
		// Config.UnitTests.ListTests = true;
		Config.UnitTests.TargetFramework = "netcoreapp2.0";
		
		//Config.UnitTests.ParameterArguments.Add("CollectCoverage=true");
		//Config.UnitTests.ParameterArguments.Add("CoverletOutputFormat=opencover");
		//Config.UnitTests.ParameterArguments.Add("CoverletOutput=./coverage.xml");
		//Config.UnitTests.SonarProjectKey = "StandardDot:branch";
		//Config.UnitTests.ReportsPaths = "/src/*Tests/coverage.xml";
		//Config.UnitTests.SonarExclusions = "**Tests**.cs";
	});