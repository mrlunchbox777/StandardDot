# Set up all the variables
if [ -f /var/lib/jenkins/.standarddotbuild.props ]; then
	rm /var/lib/jenkins/.standarddotbuild.props
fi

echo "Creating Build props"

touch /var/lib/jenkins/.standarddotbuild.props

# Branch
currentString="echo \"CI_COMMIT_REF_NAME=${GIT_BRANCH}\" >> /var/lib/jenkins/.standarddotbuild.props"
sh -c "$currentString"

# Commit
currentString="echo \"CI_COMMIT_SHA=${GIT_COMMIT}\" >> /var/lib/jenkins/.standarddotbuild.props"
sh -c "$currentString"

# Nuget
currentString="echo \"CAKE_ROSLYN_NUGETSOURCE=https://www.nuget.org\" >> /var/lib/jenkins/.standarddotbuild.props"
sh -c "$currentString"

# Repository URL
currentString="echo \"CI_REPOSITORY_URL=${GIT_URL}\" >> /var/lib/jenkins/.standarddotbuild.props"
sh -c "$currentString"

# AdditionalSubDir
# currentString="echo \"AdditionalSubDir=src/\" >> /var/lib/jenkins/.standarddotbuild.props"
# sh -c "$currentString"

# Repository URL
currentString="echo \"AddProjectDir=true\" >> /var/lib/jenkins/.standarddotbuild.props"
sh -c "$currentString"

# WORKSPACE this is the same for gitlab and jenkins

# These don't need to be set, they have good defaults or other ways they are set
# PROJECT_TO_BUILD
# PROJECTNAME
# MSBUILDOUTPUTDIR