﻿# Simple Docker Api

## Commands

```bash
# build project image
docker-compose -f docker-compose.ci.build.yml up

# run stack
docker-compose up

# rebuild runtime image
docker-compose build

# publishes image(s) to docker repository
docker-compose push
```

## Swarm Commands

```bash
# initialize swarm
docker swarm init

# deploy stack
docker stack deploy -c docker-compose.yml slabs-api

# when deploying using private repos
docker stack deploy -c docker-compose.yml slabs-api --with-registry-auth

# combine configs - this will take overrides etc...
docker-compose config > stack.yml

# scale service
docker service scale slabs-api_api=3
```

## Swarm Inspect Commands

```bash
# list stacks
docker stack ls

# list services for stack
docker stack services slabs-api
```

## App
Navigate to http://localhost:5010/api/app

or use curl

```bash
curl http://localhost:5010/api/app
```