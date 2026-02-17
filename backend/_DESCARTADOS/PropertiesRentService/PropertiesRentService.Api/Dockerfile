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
COPY PropertiesRentService/PropertiesRentService.Domain/ /app/PropertiesRentService/PropertiesRentService.Domain/
COPY PropertiesRentService/PropertiesRentService.Application/ /app/PropertiesRentService/PropertiesRentService.Application/
COPY PropertiesRentService/PropertiesRentService.Infrastructure/ /app/PropertiesRentService/PropertiesRentService.Infrastructure/
COPY PropertiesRentService/PropertiesRentService.Shared/ /app/PropertiesRentService/PropertiesRentService.Shared/
COPY PropertiesRentService/PropertiesRentService.Api/ /app/PropertiesRentService/PropertiesRentService.Api/

WORKDIR /app/PropertiesRentService/PropertiesRentService.Api

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

ENTRYPOINT ["dotnet", "PropertiesRentService.Api.dll"]
