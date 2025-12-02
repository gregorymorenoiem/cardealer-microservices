FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install PostgreSQL client tools for pg_dump and pg_restore
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BackupDRService.Api/BackupDRService.Api.csproj", "BackupDRService.Api/"]
COPY ["BackupDRService.Core/BackupDRService.Core.csproj", "BackupDRService.Core/"]
RUN dotnet restore "BackupDRService.Api/BackupDRService.Api.csproj"
COPY . .
WORKDIR "/src/BackupDRService.Api"
RUN dotnet build "BackupDRService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BackupDRService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create backup directory
RUN mkdir -p /var/backups/cardealer && chmod 755 /var/backups/cardealer

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackupDRService.Api.dll"]
