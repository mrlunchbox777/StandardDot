cd ..

GIT_BRANCH_NAME="${Build.SourceBranchName}"
if [ "${GIT_BRANCH_NAME}" == "" ]
then
    GIT_BRANCH_NAME="$(git rev-parse --abbrev-ref HEAD)"
fi
GIT_BRANCH_TARGET="develop"
if [ "${GIT_BRANCH_NAME}" == "develop" ]
then
    GIT_BRANCH_TARGET="master"
fi
export GIT_BRANCH_NAME="${GIT_BRANCH_NAME}"
export GIT_BRANCH_TARGET="${GIT_BRANCH_TARGET}"

docker-compose -f docker-compose.yml up --abort-on-container-exit $@

cd src