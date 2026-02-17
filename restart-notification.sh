#!/bin/bash
docker rm -f notificationservice 2>/dev/null || true

docker run -d \
  --name notificationservice \
  --network cardealer-microservices_cargurus-net \
  -p 15040:80 \
  -e ASPNETCORE_ENVIRONMENT=Docker \
  -e 'Database__ConnectionStrings__PostgreSQL=Host=postgres_db;Port=5432;Database=notificationservice;Username=postgres;Password=CarDealer2024!' \
  -e RabbitMQ__Host=rabbitmq \
  -e RabbitMQ__Port=5672 \
  -e RabbitMQ__Username=guest \
  -e RabbitMQ__Password=guest \
  -e Resend__ApiKey=re_Bi3rubbH_LTnrn4UDrKQqUsLiajeJimvi \
  -e Resend__FromEmail=noreply@okla.com.do \
  cardealer-notificationservice:latest

echo "NotificationService reiniciado"
docker ps | grep notification
