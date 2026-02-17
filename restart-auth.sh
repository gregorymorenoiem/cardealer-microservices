#!/bin/bash
docker stop authservice 2>/dev/null
docker rm authservice 2>/dev/null
docker run -d --name authservice \
  --network cardealer-microservices_cargurus-net \
  -p 15011:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e EnvironmentName=Docker \
  -e "ConnectionStrings__DefaultConnection=Host=postgres_db;Port=5432;Database=authservice;Username=postgres;Password=CarDealer2024!" \
  -e "JwtSettings__SecretKey=CarDealerSecretKeyForJwtToken2024MustBeAtLeast32CharactersLong!" \
  -e JwtSettings__Issuer=OKLA \
  -e JwtSettings__Audience=OKLA.Users \
  -e JwtSettings__ExpirationInMinutes=60 \
  -e RabbitMQ__Host=rabbitmq \
  -e RabbitMQ__HostName=rabbitmq \
  -e RabbitMQ__Username=guest \
  -e RabbitMQ__UserName=guest \
  -e RabbitMQ__Password=guest \
  -e RabbitMQ__VirtualHost=/ \
  -e RabbitMQ__Port=5672 \
  -e RabbitMQ__Enabled=true \
  -e FrontendSettings__BaseUrl=http://localhost:3000 \
  cardealer-authservice:latest

echo "AuthService restarted"
docker ps | grep authservice
