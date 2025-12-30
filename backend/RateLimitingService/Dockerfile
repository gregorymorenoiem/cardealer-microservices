# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy csproj files and restore
COPY ["RateLimitingService/RateLimitingService.Api/RateLimitingService.Api.csproj", "RateLimitingService/RateLimitingService.Api/"]
COPY ["RateLimitingService/RateLimitingService.Core/RateLimitingService.Core.csproj", "RateLimitingService/RateLimitingService.Core/"]
COPY ["RateLimitingService/RateLimitingService.Infrastructure/RateLimitingService.Infrastructure.csproj", "RateLimitingService/RateLimitingService.Infrastructure/"]
RUN dotnet restore "RateLimitingService/RateLimitingService.Api/RateLimitingService.Api.csproj"

# Copy everything else and build
COPY _Shared/ ./_Shared/
COPY RateLimitingService/ ./RateLimitingService/
WORKDIR "/src/RateLimitingService/RateLimitingService.Api"
RUN dotnet build "RateLimitingService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RateLimitingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Create secrets directory
RUN mkdir -p /run/secrets

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Docker

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "RateLimitingService.Api.dll"]
