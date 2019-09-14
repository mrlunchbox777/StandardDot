docker-compose -f shell.docker-compose.yml up -d $@
sleep 3s && docker-compose -f shell.docker-compose.yml exec terminal bash
docker-compose -f shell.docker-compose.yml down