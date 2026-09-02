#!/bin/bash

docker exec -it pgdb bash -c "psql -U taskory -f /docker-entrypoint-initdb.d/schema.sql"
