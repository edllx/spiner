FROM postgres:17

COPY ./database/Config/schema.sql /docker-entrypoint-initdb.d/

RUN chmod +r /docker-entrypoint-initdb.d/*.sql
