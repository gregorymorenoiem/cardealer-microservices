############################################
# PRODUCTION BUILD - Multi-stage Dockerfile
############################################

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first (for better layer caching)
COPY _Shared/ _Shared/

# Copy service projects
COPY AuthService/AuthService.Domain/ AuthService/AuthService.Domain/
COPY AuthService/AuthService.Application/ AuthService/AuthService.Application/
COPY AuthService/AuthService.Infrastructure/ AuthService/AuthService.Infrastructure/
COPY AuthService/AuthService.Api/ AuthService/AuthService.Api/

# Copy ServiceDiscovery dependency
COPY ServiceDiscovery/ServiceDiscovery.Application/ ServiceDiscovery/ServiceDiscovery.Application/
COPY ServiceDiscovery/ServiceDiscovery.Infrastructure/ ServiceDiscovery/ServiceDiscovery.Infrastructure/

# Restore dependencies
WORKDIR /src/AuthService/AuthService.Api
RUN dotnet restore

# Build
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Environment
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80

EXPOSE 80

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
