FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5012
ENV ASPNETCORE_URLS=http://+:5012
ENV ASPNETCORE_ENVIRONMENT=Docker

# Create uploads directory and secrets mount point
RUN mkdir -p /app/uploads && chmod 777 /app/uploads && \
    mkdir -p /run/secrets

# Install wget for healthcheck and ffmpeg for multimedia processing
RUN apt-get update && apt-get install -y --no-install-recommends wget ffmpeg && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy project files
COPY ["FileStorageService/FileStorageService.Api/FileStorageService.Api.csproj", "FileStorageService/FileStorageService.Api/"]
COPY ["FileStorageService/FileStorageService.Core/FileStorageService.Core.csproj", "FileStorageService/FileStorageService.Core/"]

# Restore dependencies
RUN dotnet restore "FileStorageService/FileStorageService.Api/FileStorageService.Api.csproj"

# Copy shared source code
COPY ["_Shared/", "_Shared/"]

# Copy service source code
COPY ["FileStorageService/", "FileStorageService/"]

# Build
WORKDIR "/src/FileStorageService/FileStorageService.Api"
RUN dotnet build "FileStorageService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FileStorageService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser:appuser /app /app/uploads
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5012/health || exit 1

ENTRYPOINT ["dotnet", "FileStorageService.Api.dll"]
