FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

COPY ["FeatureToggleService/FeatureToggleService.Api/FeatureToggleService.Api.csproj", "FeatureToggleService/FeatureToggleService.Api/"]
COPY ["FeatureToggleService/FeatureToggleService.Application/FeatureToggleService.Application.csproj", "FeatureToggleService/FeatureToggleService.Application/"]
COPY ["FeatureToggleService/FeatureToggleService.Domain/FeatureToggleService.Domain.csproj", "FeatureToggleService/FeatureToggleService.Domain/"]
COPY ["FeatureToggleService/FeatureToggleService.Infrastructure/FeatureToggleService.Infrastructure.csproj", "FeatureToggleService/FeatureToggleService.Infrastructure/"]
RUN dotnet restore "FeatureToggleService/FeatureToggleService.Api/FeatureToggleService.Api.csproj"

# Copy shared source code
COPY ["_Shared/", "_Shared/"]

COPY . .
WORKDIR "/src/FeatureToggleService/FeatureToggleService.Api"
RUN dotnet build "FeatureToggleService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FeatureToggleService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create secrets directory for Docker secrets
RUN mkdir -p /run/secrets

# Set environment to Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "FeatureToggleService.Api.dll"]
