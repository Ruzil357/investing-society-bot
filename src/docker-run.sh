#!/bin/bash

docker stop $(docker ps -a -q) && docker rm $(docker ps -a -q);

docker image prune -a -f --filter "until=12h"

docker build --tag hack-bot:latest .

docker run -d hack-bot:latest
