#!/bin/bash
# =============================================================================
# Initialize Multiple PostgreSQL Databases
# =============================================================================
# This script creates multiple databases for microservices from a single
# PostgreSQL instance. It reads the POSTGRES_MULTIPLE_DATABASES environment
# variable which should be a comma-separated list of database names.
# =============================================================================

set -e
set -u

function create_database() {
    local database=$1
    echo "Creating database: $database"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
        SELECT 'CREATE DATABASE $database'
        WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$database')\gexec
EOSQL
    echo "Database '$database' created or already exists."
}

if [ -n "${POSTGRES_MULTIPLE_DATABASES:-}" ]; then
    echo "Multiple database creation requested: $POSTGRES_MULTIPLE_DATABASES"
    for db in $(echo $POSTGRES_MULTIPLE_DATABASES | tr ',' ' '); do
        create_database $db
    done
    echo "All databases created successfully!"
else
    echo "No POSTGRES_MULTIPLE_DATABASES environment variable set. Skipping."
fi
