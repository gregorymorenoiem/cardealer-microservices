FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared libraries
COPY _Shared/CarDealer.Shared/CarDealer.Shared.csproj _Shared/CarDealer.Shared/
COPY _Shared/CarDealer.Contracts/CarDealer.Contracts.csproj _Shared/CarDealer.Contracts/

# Copy csproj files
COPY ApiDocsService/ApiDocsService.Core/ApiDocsService.Core.csproj ApiDocsService/ApiDocsService.Core/
COPY ApiDocsService/ApiDocsService.Api/ApiDocsService.Api.csproj ApiDocsService/ApiDocsService.Api/

# Restore dependencies
RUN dotnet restore ApiDocsService/ApiDocsService.Api/ApiDocsService.Api.csproj

# Copy source code
COPY _Shared/ _Shared/
COPY ApiDocsService/ ApiDocsService/

# Build
WORKDIR /src/ApiDocsService/ApiDocsService.Api
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create secrets directory
RUN mkdir -p /run/secrets

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Docker

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "ApiDocsService.Api.dll"]
