# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["ErrorService/ErrorService.Api/ErrorService.Api.csproj", "ErrorService/ErrorService.Api/"]
COPY ["ErrorService/ErrorService.Application/ErrorService.Application.csproj", "ErrorService/ErrorService.Application/"]
COPY ["ErrorService/ErrorService.Domain/ErrorService.Domain.csproj", "ErrorService/ErrorService.Domain/"]
COPY ["ErrorService/ErrorService.Infrastructure/ErrorService.Infrastructure.csproj", "ErrorService/ErrorService.Infrastructure/"]
COPY ["ErrorService/ErrorService.Shared/ErrorService.Shared.csproj", "ErrorService/ErrorService.Shared/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Domain/ServiceDiscovery.Domain.csproj", "ServiceDiscovery/ServiceDiscovery.Domain/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Application/ServiceDiscovery.Application.csproj", "ServiceDiscovery/ServiceDiscovery.Application/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Infrastructure/ServiceDiscovery.Infrastructure.csproj", "ServiceDiscovery/ServiceDiscovery.Infrastructure/"]

RUN dotnet restore "ErrorService/ErrorService.Api/ErrorService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ErrorService/ErrorService.Api"
RUN dotnet build "ErrorService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ErrorService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 80

# Create directories for secrets
RUN mkdir -p /run/secrets

# Create non-root user (Alpine syntax)
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app /run/secrets

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:80

# Health check using wget
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "ErrorService.Api.dll"]
