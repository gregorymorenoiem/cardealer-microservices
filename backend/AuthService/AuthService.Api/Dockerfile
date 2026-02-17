############################################
# PRODUCTION Dockerfile - Based on Dockerfile.dev
############################################

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file for restore (from Dockerfile.dev)
COPY AuthService/AuthService.Api/AuthService.Api.csproj ./AuthService/AuthService.Api/

# Restore packages
RUN dotnet restore "AuthService/AuthService.Api/AuthService.Api.csproj"

# Copy all source code (same as Dockerfile.dev: COPY . .)
COPY . .

# Build in Release mode
RUN dotnet build "AuthService/AuthService.Api/AuthService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "AuthService/AuthService.Api/AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime (slim image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Production environment
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
