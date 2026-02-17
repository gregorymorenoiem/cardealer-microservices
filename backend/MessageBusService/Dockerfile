# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MessageBusService/MessageBusService.Api/MessageBusService.Api.csproj", "MessageBusService/MessageBusService.Api/"]
COPY ["MessageBusService/MessageBusService.Application/MessageBusService.Application.csproj", "MessageBusService/MessageBusService.Application/"]
COPY ["MessageBusService/MessageBusService.Infrastructure/MessageBusService.Infrastructure.csproj", "MessageBusService/MessageBusService.Infrastructure/"]
COPY ["MessageBusService/MessageBusService.Domain/MessageBusService.Domain.csproj", "MessageBusService/MessageBusService.Domain/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Application/ServiceDiscovery.Application.csproj", "ServiceDiscovery/ServiceDiscovery.Application/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Domain/ServiceDiscovery.Domain.csproj", "ServiceDiscovery/ServiceDiscovery.Domain/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Infrastructure/ServiceDiscovery.Infrastructure.csproj", "ServiceDiscovery/ServiceDiscovery.Infrastructure/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Restore dependencies
RUN dotnet restore "MessageBusService/MessageBusService.Api/MessageBusService.Api.csproj"

# Copy everything else
COPY MessageBusService/ MessageBusService/
COPY ServiceDiscovery/ ServiceDiscovery/
COPY _Shared/ _Shared/

# Build
WORKDIR "/src/MessageBusService/MessageBusService.Api"
RUN dotnet build "MessageBusService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "MessageBusService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - Alpine para menor superficie de ataque
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Install wget for healthcheck and create secrets directory
RUN apk add --no-cache wget && \
    mkdir -p /run/secrets

# Create non-root user (Alpine uses addgroup/adduser)
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app /run/secrets

# Set environment to Docker
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 http://localhost:8080/health -qO- > /dev/null || exit 1

ENTRYPOINT ["dotnet", "MessageBusService.Api.dll"]
