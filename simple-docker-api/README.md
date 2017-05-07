# Simple Docker Api

## Commands

```bash
# build project image
docker-compose -f docker-compose.ci.build.yml up

# run stack
docker-compose up

# rebuild runtime image
docker-compose build
```

## Swarm Commands

```bash
# initialize swarm
docker swarm init

# deploy stack
docker stack deploy -c docker-compose.yml slabs-api

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