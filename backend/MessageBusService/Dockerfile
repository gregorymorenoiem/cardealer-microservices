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

# Restore dependencies
RUN dotnet restore "MessageBusService/MessageBusService.Api/MessageBusService.Api.csproj"

# Copy everything else
COPY MessageBusService/ MessageBusService/
COPY ServiceDiscovery/ ServiceDiscovery/

# Build
WORKDIR "/src/MessageBusService/MessageBusService.Api"
RUN dotnet build "MessageBusService.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "MessageBusService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - Alpine para menor superficie de ataque
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Create non-root user (Alpine uses addgroup/adduser)
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD dotnet /app/MessageBusService.Api.dll --help > /dev/null 2>&1 || exit 1

ENTRYPOINT ["dotnet", "MessageBusService.Api.dll"]
