#!/bin/bash
# Creates one database per microservice on the shared Postgres instance used for
# Phase 1 local development. In a production deployment each service would get its
# own managed Postgres instance instead of sharing a container.
set -e

for db in msecs_identity msecs_site msecs_asset msecs_device_registry; do
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    SELECT 'CREATE DATABASE $db' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db')\gexec
EOSQL
done
