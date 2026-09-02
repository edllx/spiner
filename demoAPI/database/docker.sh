#!/bin/bash

docker compose down
docker compose build --build-arg CACHE_BUSTER=$(date +%s)
docker compose up -d --no-build
docker exec -it pgdb bash -c "while ! pg_isready -U taskory; do sleep 2; done &&
psql -U weddy -f /docker-entrypoint-initdb.d/schema.sql"
