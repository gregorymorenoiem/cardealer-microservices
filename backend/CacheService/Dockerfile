FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5095
EXPOSE 7095

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Create secrets directory
RUN mkdir -p /run/secrets

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy ServiceDiscovery projects
COPY ["ServiceDiscovery/ServiceDiscovery.Domain/ServiceDiscovery.Domain.csproj", "ServiceDiscovery/ServiceDiscovery.Domain/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Application/ServiceDiscovery.Application.csproj", "ServiceDiscovery/ServiceDiscovery.Application/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Infrastructure/ServiceDiscovery.Infrastructure.csproj", "ServiceDiscovery/ServiceDiscovery.Infrastructure/"]

# Copy csproj files and restore dependencies
COPY ["CacheService/CacheService.Api/CacheService.Api.csproj", "CacheService/CacheService.Api/"]
COPY ["CacheService/CacheService.Application/CacheService.Application.csproj", "CacheService/CacheService.Application/"]
COPY ["CacheService/CacheService.Domain/CacheService.Domain.csproj", "CacheService/CacheService.Domain/"]
COPY ["CacheService/CacheService.Infrastructure/CacheService.Infrastructure.csproj", "CacheService/CacheService.Infrastructure/"]

RUN dotnet restore "CacheService/CacheService.Api/CacheService.Api.csproj"

# Copy all source files
COPY _Shared/ ./_Shared/
COPY ServiceDiscovery/ ./ServiceDiscovery/
COPY CacheService/ ./CacheService/

# Build the application
WORKDIR "/src/CacheService/CacheService.Api"
RUN dotnet build "CacheService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CacheService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Set Docker environment
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:5095/health || exit 1

ENTRYPOINT ["dotnet", "CacheService.Api.dll"]
