FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5096
EXPOSE 7096

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Create secrets directory
RUN mkdir -p /run/secrets

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy csproj files and restore dependencies
COPY ["IdempotencyService/IdempotencyService.Api/IdempotencyService.Api.csproj", "IdempotencyService/IdempotencyService.Api/"]
COPY ["IdempotencyService/IdempotencyService.Core/IdempotencyService.Core.csproj", "IdempotencyService/IdempotencyService.Core/"]

RUN dotnet restore "IdempotencyService/IdempotencyService.Api/IdempotencyService.Api.csproj"

# Copy all source files
COPY _Shared/ ./_Shared/
COPY IdempotencyService/ ./IdempotencyService/

# Build the application
WORKDIR "/src/IdempotencyService/IdempotencyService.Api"
RUN dotnet build "IdempotencyService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "IdempotencyService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Set Docker environment
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:5096

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:5096/health || exit 1

ENTRYPOINT ["dotnet", "IdempotencyService.Api.dll"]
