# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Application/ServiceDiscovery.Application.csproj", "ServiceDiscovery/ServiceDiscovery.Application/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Infrastructure/ServiceDiscovery.Infrastructure.csproj", "ServiceDiscovery/ServiceDiscovery.Infrastructure/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Domain/ServiceDiscovery.Domain.csproj", "ServiceDiscovery/ServiceDiscovery.Domain/"]

# Copy csproj files and restore
COPY ["MediaService/MediaService.Api/MediaService.Api.csproj", "MediaService/MediaService.Api/"]
COPY ["MediaService/MediaService.Application/MediaService.Application.csproj", "MediaService/MediaService.Application/"]
COPY ["MediaService/MediaService.Domain/MediaService.Domain.csproj", "MediaService/MediaService.Domain/"]
COPY ["MediaService/MediaService.Infrastructure/MediaService.Infrastructure.csproj", "MediaService/MediaService.Infrastructure/"]
COPY ["MediaService/MediaService.Shared/MediaService.Shared.csproj", "MediaService/MediaService.Shared/"]

RUN dotnet restore "MediaService/MediaService.Api/MediaService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MediaService/MediaService.Api"
RUN dotnet build "MediaService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MediaService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for health checks
RUN apt-get update && apt-get install -y --no-install-recommends wget && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user, uploads directory and secrets directory
RUN useradd -m -u 1000 appuser && \
    mkdir -p /app/uploads && \
    mkdir -p /run/secrets && \
    chown -R appuser:appuser /app /run/secrets
USER appuser

# Set Docker environment
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "MediaService.Api.dll"]