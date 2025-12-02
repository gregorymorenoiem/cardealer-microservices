FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ConfigurationService projects
COPY ["ConfigurationService/ConfigurationService.Api/ConfigurationService.Api.csproj", "ConfigurationService/ConfigurationService.Api/"]
COPY ["ConfigurationService/ConfigurationService.Application/ConfigurationService.Application.csproj", "ConfigurationService/ConfigurationService.Application/"]
COPY ["ConfigurationService/ConfigurationService.Domain/ConfigurationService.Domain.csproj", "ConfigurationService/ConfigurationService.Domain/"]
COPY ["ConfigurationService/ConfigurationService.Infrastructure/ConfigurationService.Infrastructure.csproj", "ConfigurationService/ConfigurationService.Infrastructure/"]

# Copy ServiceDiscovery projects (required dependency)
COPY ["ServiceDiscovery/ServiceDiscovery.Domain/ServiceDiscovery.Domain.csproj", "ServiceDiscovery/ServiceDiscovery.Domain/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Application/ServiceDiscovery.Application.csproj", "ServiceDiscovery/ServiceDiscovery.Application/"]
COPY ["ServiceDiscovery/ServiceDiscovery.Infrastructure/ServiceDiscovery.Infrastructure.csproj", "ServiceDiscovery/ServiceDiscovery.Infrastructure/"]

# Restore packages
RUN dotnet restore "ConfigurationService/ConfigurationService.Api/ConfigurationService.Api.csproj"

# Copy all source code
COPY ConfigurationService/ ConfigurationService/
COPY ServiceDiscovery/ ServiceDiscovery/

# Build
WORKDIR "/src/ConfigurationService/ConfigurationService.Api"
RUN dotnet build "ConfigurationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConfigurationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConfigurationService.Api.dll"]
