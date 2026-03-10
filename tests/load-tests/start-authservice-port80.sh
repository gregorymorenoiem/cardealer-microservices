#!/bin/bash
# Start AuthService matching ocelot Docker config (port 80)
docker run -d \
  --name authservice \
  --hostname authservice \
  --network cardealer-microservices_cardealer-net \
  --network-alias authservice \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e 'ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=authservice;Username=postgres;Password=CarDealerDBPassword2026Secure!' \
  -e 'Redis__Connection=redis:6379,password=RedisPassword2026Secure!,abortConnect=false' \
  -e RabbitMQ__Enabled=false \
  -e Consul__Enabled=false \
  -e Jwt__Key=TM9Gd7Kh3r31M8alRKbZr+SkXMALx7SJmVxWeBbyUCrI5oTilVoeaooMZSc3KzGF \
  -e Jwt__Issuer=CarDealer \
  -e Jwt__Audience=CarDealerApp \
  ghcr.io/gregorymorenoiem/cardealer-authservice:latest
echo "AuthService started on port 80 (default)"
