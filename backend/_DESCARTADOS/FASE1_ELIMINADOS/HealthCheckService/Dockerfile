# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy csproj files
COPY ["HealthCheckService/HealthCheckService.Api/HealthCheckService.Api.csproj", "HealthCheckService/HealthCheckService.Api/"]
COPY ["HealthCheckService/HealthCheckService.Infrastructure/HealthCheckService.Infrastructure.csproj", "HealthCheckService/HealthCheckService.Infrastructure/"]
COPY ["HealthCheckService/HealthCheckService.Application/HealthCheckService.Application.csproj", "HealthCheckService/HealthCheckService.Application/"]
COPY ["HealthCheckService/HealthCheckService.Domain/HealthCheckService.Domain.csproj", "HealthCheckService/HealthCheckService.Domain/"]

# Restore dependencies
RUN dotnet restore "HealthCheckService/HealthCheckService.Api/HealthCheckService.Api.csproj"

# Copy everything else
COPY ["_Shared/", "_Shared/"]
COPY ["HealthCheckService/", "HealthCheckService/"]

# Build
WORKDIR "/src/HealthCheckService/HealthCheckService.Api"
RUN dotnet build "HealthCheckService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HealthCheckService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for healthcheck and create secrets directory
RUN apt-get update && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /run/secrets

# Set Docker environment
ENV ASPNETCORE_ENVIRONMENT=Docker

# Create non-root user
RUN useradd -m -u 1000 healthcheckservice \
    && chown -R healthcheckservice:healthcheckservice /app \
    && chown -R healthcheckservice:healthcheckservice /run/secrets
USER healthcheckservice

COPY --from=publish --chown=healthcheckservice:healthcheckservice /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "HealthCheckService.Api.dll"]
