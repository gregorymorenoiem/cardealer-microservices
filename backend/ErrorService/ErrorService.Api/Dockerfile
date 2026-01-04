############################################
# PRODUCTION BUILD - ErrorService Multi-stage Dockerfile
############################################

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY _Shared/ _Shared/

# Copy service projects
COPY ErrorService/ErrorService.Domain/ ErrorService/ErrorService.Domain/
COPY ErrorService/ErrorService.Application/ ErrorService/ErrorService.Application/
COPY ErrorService/ErrorService.Infrastructure/ ErrorService/ErrorService.Infrastructure/
COPY ErrorService/ErrorService.Api/ ErrorService/ErrorService.Api/

# Restore dependencies
WORKDIR /src/ErrorService/ErrorService.Api
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

ENTRYPOINT ["dotnet", "ErrorService.Api.dll"]
