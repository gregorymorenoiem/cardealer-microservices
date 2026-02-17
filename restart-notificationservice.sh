#!/bin/bash

# Remove existing container
docker rm -f notificationservice 2>/dev/null || true

# Start NotificationService
docker run -d --name notificationservice \
  --network cardealer-microservices_cargurus-net \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e "Database__ConnectionStrings__PostgreSQL=Host=postgres_db;Port=5432;Database=notificationservice;Username=postgres;Password=CarDealer2024!" \
  -e RabbitMQ__Host=rabbitmq \
  -e RabbitMQ__Port=5672 \
  -e RabbitMQ__Username=guest \
  -e RabbitMQ__Password=guest \
  -p 15040:80 \
  cardealer-notificationservice:latest

echo "NotificationService started. Waiting for health check..."
sleep 5
curl -s http://localhost:15040/health
