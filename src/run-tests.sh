cd ..

GIT_BRANCH_NAME="${BUILD_SOURCEBRANCHNAME}"
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

echo "------------------------------------------------------------"
echo "Running tests for potential merge operation"
echo "${GIT_BRANCH_NAME} (Source) -> ${GIT_BRANCH_TARGET} (Target)"
echo "NOTE: if master, not publishing target to sonarqube per requirements"
echo "------------------------------------------------------------"

docker-compose -f docker-compose.yml up --abort-on-container-exit $@

cd src