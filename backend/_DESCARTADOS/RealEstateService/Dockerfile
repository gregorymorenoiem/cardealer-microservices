# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy csproj files
COPY ["RealEstateService.Api/RealEstateService.Api.csproj", "RealEstateService.Api/"]
COPY ["RealEstateService.Application/RealEstateService.Application.csproj", "RealEstateService.Application/"]
COPY ["RealEstateService.Domain/RealEstateService.Domain.csproj", "RealEstateService.Domain/"]
COPY ["RealEstateService.Infrastructure/RealEstateService.Infrastructure.csproj", "RealEstateService.Infrastructure/"]

# Restore
RUN dotnet restore "RealEstateService.Api/RealEstateService.Api.csproj"

# Copy shared source
COPY ["_Shared/", "_Shared/"]

# Copy everything
COPY . .

# Build
WORKDIR "/src/RealEstateService.Api"
RUN dotnet build "RealEstateService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RealEstateService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create secrets directory for Docker secrets
RUN mkdir -p /run/secrets

# Set Docker environment
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

# Install wget for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "RealEstateService.Api.dll"]
