cd ..
docker-compose -f update-packages.docker-compose.yml up $@
docker-compose -f update-packages.docker-compose.yml down
cd src
echo ready!