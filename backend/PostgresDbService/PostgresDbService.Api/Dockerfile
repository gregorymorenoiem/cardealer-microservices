FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for better layer caching
COPY ["PostgresDbService/PostgresDbService.Api/PostgresDbService.Api.csproj", "PostgresDbService/PostgresDbService.Api/"]
COPY ["PostgresDbService/PostgresDbService.Infrastructure/PostgresDbService.Infrastructure.csproj", "PostgresDbService/PostgresDbService.Infrastructure/"]
COPY ["PostgresDbService/PostgresDbService.Domain/PostgresDbService.Domain.csproj", "PostgresDbService/PostgresDbService.Domain/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Restore packages
RUN dotnet restore "PostgresDbService/PostgresDbService.Api/PostgresDbService.Api.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/PostgresDbService/PostgresDbService.Api"
RUN dotnet build "PostgresDbService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PostgresDbService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PostgresDbService.Api.dll"]