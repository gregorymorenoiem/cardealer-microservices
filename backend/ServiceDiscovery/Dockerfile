# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ServiceDiscovery.Api/ServiceDiscovery.Api.csproj", "ServiceDiscovery.Api/"]
COPY ["ServiceDiscovery.Application/ServiceDiscovery.Application.csproj", "ServiceDiscovery.Application/"]
COPY ["ServiceDiscovery.Domain/ServiceDiscovery.Domain.csproj", "ServiceDiscovery.Domain/"]
COPY ["ServiceDiscovery.Infrastructure/ServiceDiscovery.Infrastructure.csproj", "ServiceDiscovery.Infrastructure/"]

RUN dotnet restore "ServiceDiscovery.Api/ServiceDiscovery.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ServiceDiscovery.Api"
RUN dotnet build "ServiceDiscovery.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ServiceDiscovery.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl --fail http://localhost:80/api/health || exit 1

ENTRYPOINT ["dotnet", "ServiceDiscovery.Api.dll"]
