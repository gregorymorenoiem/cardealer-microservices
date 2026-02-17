# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY AIProcessingService.Api/AIProcessingService.Api.csproj AIProcessingService.Api/
COPY AIProcessingService.Application/AIProcessingService.Application.csproj AIProcessingService.Application/
COPY AIProcessingService.Infrastructure/AIProcessingService.Infrastructure.csproj AIProcessingService.Infrastructure/
COPY AIProcessingService.Domain/AIProcessingService.Domain.csproj AIProcessingService.Domain/

# Restore dependencies
RUN dotnet restore AIProcessingService.Api/AIProcessingService.Api.csproj

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/AIProcessingService.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

# Copy published files
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AIProcessingService.Api.dll"]
