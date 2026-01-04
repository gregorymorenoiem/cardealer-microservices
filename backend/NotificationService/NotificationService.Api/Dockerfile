############################################
# PRODUCTION BUILD - NotificationService Multi-stage Dockerfile
############################################

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY _Shared/ _Shared/

# Copy service projects
COPY NotificationService/NotificationService.Domain/ NotificationService/NotificationService.Domain/
COPY NotificationService/NotificationService.Application/ NotificationService/NotificationService.Application/
COPY NotificationService/NotificationService.Infrastructure/ NotificationService/NotificationService.Infrastructure/
COPY NotificationService/NotificationService.Api/ NotificationService/NotificationService.Api/

# Restore dependencies
WORKDIR /src/NotificationService/NotificationService.Api
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

ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
