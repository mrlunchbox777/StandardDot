cd ..
docker-compose -f shell.docker-compose.yml build $@
docker-compose -f docker-compose.yml build $@
cd src
echo ready!