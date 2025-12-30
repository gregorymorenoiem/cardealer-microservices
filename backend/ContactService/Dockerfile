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
COPY ["ContactService/ContactService.Api/ContactService.Api.csproj", "ContactService/ContactService.Api/"]
COPY ["ContactService/ContactService.Application/ContactService.Application.csproj", "ContactService/ContactService.Application/"]
COPY ["ContactService/ContactService.Domain/ContactService.Domain.csproj", "ContactService/ContactService.Domain/"]
COPY ["ContactService/ContactService.Infrastructure/ContactService.Infrastructure.csproj", "ContactService/ContactService.Infrastructure/"]

RUN dotnet restore "ContactService/ContactService.Api/ContactService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ContactService/ContactService.Api"
RUN dotnet build "ContactService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ContactService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Create secrets directory for Docker secrets
RUN mkdir -p /run/secrets

# Set environment to Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app /run/secrets
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "ContactService.Api.dll"]
