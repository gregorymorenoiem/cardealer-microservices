#!/bin/bash
# Start AuthService for load testing
docker run -d --name authservice-loadtest \
  --network cardealer-microservices_cardealer-net \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e "ASPNETCORE_URLS=http://+:8080" \
  -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=authservice;Username=postgres;Password=CarDealerDBPassword2026Secure!" \
  -e "Redis__Connection=redis:6379,password=RedisPassword2026Secure!,abortConnect=false" \
  -e RABBITMQ_HOST=rabbitmq \
  -e RABBITMQ_PORT=5672 \
  -e RABBITMQ_USERNAME=cardealer \
  -e "CONSUL_ADDRESS=http://consul:8500" \
  -e "Jwt__Key=TM9Gd7Kh3r31M8alRKbZr+SkXMALx7SJmVxWeBbyUCrI5oTilVoeaooMZSc3KzGF" \
  -e "Jwt__Issuer=CarDealer" \
  -e "Jwt__Audience=CarDealerApp" \
  ghcr.io/gregorymorenoiem/cardealer-authservice:latest
