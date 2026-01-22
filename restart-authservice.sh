#!/bin/bash

# Remove existing container
docker rm -f authservice 2>/dev/null || true

# Start AuthService with RabbitMQ ENABLED
docker run -d --name authservice \
  --network cardealer-microservices_cargurus-net \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e JWT_SECRET_KEY=CarDealerSecretKeyForJwtToken2024MustBeAtLeast32CharactersLong! \
  -e JWT_ISSUER=OKLA \
  -e JWT_AUDIENCE=OKLA.Users \
  -e "Database__ConnectionStrings__PostgreSQL=Host=postgres_db;Port=5432;Database=authservice;Username=postgres;Password=CarDealer2024!" \
  -e RabbitMQ__Host=rabbitmq \
  -e RabbitMQ__Port=5672 \
  -e RabbitMQ__Username=guest \
  -e RabbitMQ__Password=guest \
  -e RabbitMQ__Enabled=true \
  -p 15011:80 \
  cardealer-authservice:latest

echo "AuthService started. Waiting for health check..."
sleep 5
curl -s http://localhost:15011/health
