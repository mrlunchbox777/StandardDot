docker-compose -f terminal.docker-compose.yml up --abort-on-container-exit &
sleep 10s && docker-compose -f terminal.docker-compose.yml exec terminal bash