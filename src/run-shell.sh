cd ..
docker-compose -f shell.docker-compose.yml up -d $@
sleep 3s && docker-compose -f shell.docker-compose.yml exec standarddot_shell bash
docker-compose -f shell.docker-compose.yml down
cd src