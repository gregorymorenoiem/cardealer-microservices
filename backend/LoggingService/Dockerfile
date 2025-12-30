# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy ServiceDiscovery projects
COPY ["ServiceDiscovery/ServiceDiscovery.Application/ServiceDiscovery.Application.csproj", "ServiceDiscovery/ServiceDiscovery.Application/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Domain/ServiceDiscovery.Domain.csproj", "ServiceDiscovery/ServiceDiscovery.Domain/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Infrastructure/ServiceDiscovery.Infrastructure.csproj", "ServiceDiscovery/ServiceDiscovery.Infrastructure/"]

# Copy solution and project files
COPY ["LoggingService/LoggingService.Api/LoggingService.Api.csproj", "LoggingService/LoggingService.Api/"]
COPY ["LoggingService/LoggingService.Application/LoggingService.Application.csproj", "LoggingService/LoggingService.Application/"]
COPY ["LoggingService/LoggingService.Domain/LoggingService.Domain.csproj", "LoggingService/LoggingService.Domain/"]
COPY ["LoggingService/LoggingService.Infrastructure/LoggingService.Infrastructure.csproj", "LoggingService/LoggingService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "LoggingService/LoggingService.Api/LoggingService.Api.csproj"

# Copy the rest of the code
COPY _Shared/ _Shared/
COPY ServiceDiscovery/ ServiceDiscovery/
COPY LoggingService/ LoggingService/

# Build
WORKDIR "/src/LoggingService/LoggingService.Api"
RUN dotnet build "LoggingService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "LoggingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create secrets directory for Docker secrets
RUN mkdir -p /run/secrets

# Install wget for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*

# Set environment to Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "LoggingService.Api.dll"]
