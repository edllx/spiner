podman-compose -f docker-compose.yml down -v
podman-compose -f docker-compose.yml build --build-arg CACHE_BUSTER=$(date +%s)
podman-compose -f docker-compose.yml up
