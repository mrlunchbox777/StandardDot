Cake

Documentation - http://cakebuild.net/docs/tutorials/setting-up-a-new-project

Usage
Add the Cake repository as a submodule to the repo in a folder in the root called Cake
Copy Cake/Example/Build.cake to the project directory you wish to build
Copy Cake/.(os-type).gitlab-ci.yml to the repo root and rename it to .gitlab-ci.yml
Set up the pipeline in gitlab
	Ensure that under project variable the IS_WINDOWS is set to 0 (false), or 1 (true)
Ensure that UnitTest conventions are followed