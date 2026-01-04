############################################
# PRODUCTION Dockerfile - Based on Dockerfile.dev
############################################

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy shared projects (from Dockerfile.dev)
COPY _Shared/CarDealer.Shared/ /app/_Shared/CarDealer.Shared/
COPY _Shared/CarDealer.Contracts/ /app/_Shared/CarDealer.Contracts/

# Copy service projects
COPY VehiclesRentService/VehiclesRentService.Domain/ /app/VehiclesRentService/VehiclesRentService.Domain/
COPY VehiclesRentService/VehiclesRentService.Application/ /app/VehiclesRentService/VehiclesRentService.Application/
COPY VehiclesRentService/VehiclesRentService.Infrastructure/ /app/VehiclesRentService/VehiclesRentService.Infrastructure/
COPY VehiclesRentService/VehiclesRentService.Shared/ /app/VehiclesRentService/VehiclesRentService.Shared/
COPY VehiclesRentService/VehiclesRentService.Api/ /app/VehiclesRentService/VehiclesRentService.Api/

WORKDIR /app/VehiclesRentService/VehiclesRentService.Api

# Restore and build in Release mode
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime (slim image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Production environment
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80

EXPOSE 80

ENTRYPOINT ["dotnet", "VehiclesRentService.Api.dll"]
