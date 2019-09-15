cd ..

GIT_BRANCH_NAME="$(git rev-parse --abbrev-ref HEAD)"
if [ "${GIT_BRANCH_NAME}" == "master" ]
then
    docker-compose -f publish.docker-compose.yml up $@
    docker-compose -f publish.docker-compose.yml down
else
    echo "---------------------EXITING-----------------------"
    echo "Refusing to publish on any branch other than master"
    echo "---------------------EXITING-----------------------"
fi

cd src