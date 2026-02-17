#!/bin/bash
docker run -d \
  --name authservice \
  --network cardealer-microservices_cargurus-net \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e RabbitMQ__Host=rabbitmq \
  -e 'Database__ConnectionStrings__PostgreSQL=Host=postgres_db;Port=5432;Database=authservice;Username=postgres;Password=CarDealer2024!' \
  -p 15010:80 \
  cardealer-authservice:latest

docker run -d \
  --name notificationservice \
  --network cardealer-microservices_cargurus-net \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e RabbitMQ__Host=rabbitmq \
  -e 'Database__ConnectionStrings__PostgreSQL=Host=postgres_db;Port=5432;Database=notificationservice;Username=postgres;Password=CarDealer2024!' \
  -p 15040:80 \
  cardealer-notificationservice:latest
