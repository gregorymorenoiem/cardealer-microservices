#!/bin/bash
set -e

echo "Creating databases for all microservices..."

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    -- Create all databases for microservices
    SELECT 'CREATE DATABASE errorservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'errorservice')\\gexec
    SELECT 'CREATE DATABASE authservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'authservice')\\gexec
    SELECT 'CREATE DATABASE notificationservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'notificationservice')\\gexec
    SELECT 'CREATE DATABASE userservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'userservice')\\gexec
    SELECT 'CREATE DATABASE roleservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'roleservice')\\gexec
    SELECT 'CREATE DATABASE adminservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'adminservice')\\gexec
    SELECT 'CREATE DATABASE mediaservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'mediaservice')\\gexec
    SELECT 'CREATE DATABASE reportsservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'reportsservice')\\gexec
    SELECT 'CREATE DATABASE billingservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'billingservice')\\gexec
    SELECT 'CREATE DATABASE financeservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'financeservice')\\gexec
    SELECT 'CREATE DATABASE invoicingservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'invoicingservice')\\gexec
    SELECT 'CREATE DATABASE crmservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'crmservice')\\gexec
    SELECT 'CREATE DATABASE contactservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'contactservice')\\gexec
    SELECT 'CREATE DATABASE appointmentservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'appointmentservice')\\gexec
    SELECT 'CREATE DATABASE marketingservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'marketingservice')\\gexec
    SELECT 'CREATE DATABASE realestateservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'realestateservice')\\gexec
    SELECT 'CREATE DATABASE messagebusservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'messagebusservice')\\gexec
    SELECT 'CREATE DATABASE vehiclessaleservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'vehiclessaleservice')\\gexec
    SELECT 'CREATE DATABASE auditservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'auditservice')\\gexec
    SELECT 'CREATE DATABASE backupdrservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'backupdrservice')\\gexec
    SELECT 'CREATE DATABASE schedulerservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'schedulerservice')\\gexec
    SELECT 'CREATE DATABASE configurationservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'configurationservice')\\gexec
    SELECT 'CREATE DATABASE featuretoggleservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'featuretoggleservice')\\gexec
    SELECT 'CREATE DATABASE ratelimitingservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ratelimitingservice')\\gexec
    SELECT 'CREATE DATABASE maintenanceservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'maintenanceservice')\\gexec
    SELECT 'CREATE DATABASE comparisonservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'comparisonservice')\\gexec
    SELECT 'CREATE DATABASE alertservice' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'alertservice')\\gexec
EOSQL

echo "All databases created successfully!"