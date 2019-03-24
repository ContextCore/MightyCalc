FROM postgres:latest as persistence
COPY ./build/persistence/initPostgres.sql /docker-entrypoint-initdb.d/