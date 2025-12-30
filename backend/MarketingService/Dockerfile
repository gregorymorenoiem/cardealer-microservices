FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Create secrets directory
RUN mkdir -p /run/secrets

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MarketingService/MarketingService.Api/MarketingService.Api.csproj", "MarketingService/MarketingService.Api/"]
COPY ["MarketingService/MarketingService.Application/MarketingService.Application.csproj", "MarketingService/MarketingService.Application/"]
COPY ["MarketingService/MarketingService.Domain/MarketingService.Domain.csproj", "MarketingService/MarketingService.Domain/"]
COPY ["MarketingService/MarketingService.Infrastructure/MarketingService.Infrastructure.csproj", "MarketingService/MarketingService.Infrastructure/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]
RUN dotnet restore "MarketingService/MarketingService.Api/MarketingService.Api.csproj"
COPY . .
WORKDIR "/src/MarketingService/MarketingService.Api"
RUN dotnet build "MarketingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Set Docker environment
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "MarketingService.Api.dll"]
