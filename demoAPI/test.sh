docker compose -f docker-compose.yml down -v
docker compose -f docker-compose.yml build --build-arg CACHE_BUSTER=$(date +%s)
docker compose -f docker-compose.yml up
