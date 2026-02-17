FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install PostgreSQL client tools for pg_dump and pg_restore, plus wget for healthcheck
RUN apt-get update && apt-get install -y postgresql-client wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

COPY ["BackupDRService/BackupDRService.Api/BackupDRService.Api.csproj", "BackupDRService/BackupDRService.Api/"]
COPY ["BackupDRService/BackupDRService.Core/BackupDRService.Core.csproj", "BackupDRService/BackupDRService.Core/"]
RUN dotnet restore "BackupDRService/BackupDRService.Api/BackupDRService.Api.csproj"

COPY _Shared/ ./_Shared/
COPY BackupDRService/ ./BackupDRService/

WORKDIR "/src/BackupDRService/BackupDRService.Api"
RUN dotnet build "BackupDRService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BackupDRService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create backup directory and secrets directory
RUN mkdir -p /var/backups/cardealer && chmod 755 /var/backups/cardealer
RUN mkdir -p /run/secrets

# Set environment to Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "BackupDRService.Api.dll"]
