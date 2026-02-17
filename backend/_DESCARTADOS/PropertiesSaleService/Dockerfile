# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy csproj files and restore
COPY ["PropertiesSaleService/PropertiesSaleService.Api/PropertiesSaleService.Api.csproj", "PropertiesSaleService/PropertiesSaleService.Api/"]
COPY ["PropertiesSaleService/PropertiesSaleService.Application/PropertiesSaleService.Application.csproj", "PropertiesSaleService/PropertiesSaleService.Application/"]
COPY ["PropertiesSaleService/PropertiesSaleService.Domain/PropertiesSaleService.Domain.csproj", "PropertiesSaleService/PropertiesSaleService.Domain/"]
COPY ["PropertiesSaleService/PropertiesSaleService.Infrastructure/PropertiesSaleService.Infrastructure.csproj", "PropertiesSaleService/PropertiesSaleService.Infrastructure/"]
COPY ["PropertiesSaleService/PropertiesSaleService.Shared/PropertiesSaleService.Shared.csproj", "PropertiesSaleService/PropertiesSaleService.Shared/"]

RUN dotnet restore "PropertiesSaleService/PropertiesSaleService.Api/PropertiesSaleService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/PropertiesSaleService/PropertiesSaleService.Api"
RUN dotnet build "PropertiesSaleService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PropertiesSaleService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user and secrets directory (Alpine syntax)
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    mkdir -p /run/secrets && \
    chown -R appuser:appuser /app /run/secrets

# Install wget for healthcheck
RUN apk add --no-cache wget

# Set Docker environment
ENV ASPNETCORE_ENVIRONMENT=Docker

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "PropertiesSaleService.Api.dll"]
