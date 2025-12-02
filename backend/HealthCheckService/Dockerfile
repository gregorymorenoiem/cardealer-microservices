# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["HealthCheckService.Api/HealthCheckService.Api.csproj", "HealthCheckService.Api/"]
COPY ["HealthCheckService.Infrastructure/HealthCheckService.Infrastructure.csproj", "HealthCheckService.Infrastructure/"]
COPY ["HealthCheckService.Application/HealthCheckService.Application.csproj", "HealthCheckService.Application/"]
COPY ["HealthCheckService.Domain/HealthCheckService.Domain.csproj", "HealthCheckService.Domain/"]

# Restore dependencies
RUN dotnet restore "HealthCheckService.Api/HealthCheckService.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/HealthCheckService.Api"
RUN dotnet build "HealthCheckService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HealthCheckService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user
RUN useradd -m -u 1000 healthcheckservice && chown -R healthcheckservice:healthcheckservice /app
USER healthcheckservice

COPY --from=publish --chown=healthcheckservice:healthcheckservice /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "HealthCheckService.Api.dll"]
