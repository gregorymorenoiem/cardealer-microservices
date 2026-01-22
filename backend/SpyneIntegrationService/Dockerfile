# SpyneIntegrationService
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SpyneIntegrationService.Api/SpyneIntegrationService.Api.csproj", "SpyneIntegrationService.Api/"]
COPY ["SpyneIntegrationService.Application/SpyneIntegrationService.Application.csproj", "SpyneIntegrationService.Application/"]
COPY ["SpyneIntegrationService.Domain/SpyneIntegrationService.Domain.csproj", "SpyneIntegrationService.Domain/"]
COPY ["SpyneIntegrationService.Infrastructure/SpyneIntegrationService.Infrastructure.csproj", "SpyneIntegrationService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SpyneIntegrationService.Api/SpyneIntegrationService.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/SpyneIntegrationService.Api"
RUN dotnet build "SpyneIntegrationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpyneIntegrationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SpyneIntegrationService.Api.dll"]
