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

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy service projects
COPY ["ReportsService/ReportsService.Api/ReportsService.Api.csproj", "ReportsService/ReportsService.Api/"]
COPY ["ReportsService/ReportsService.Application/ReportsService.Application.csproj", "ReportsService/ReportsService.Application/"]
COPY ["ReportsService/ReportsService.Domain/ReportsService.Domain.csproj", "ReportsService/ReportsService.Domain/"]
COPY ["ReportsService/ReportsService.Infrastructure/ReportsService.Infrastructure.csproj", "ReportsService/ReportsService.Infrastructure/"]

RUN dotnet restore "ReportsService/ReportsService.Api/ReportsService.Api.csproj"

# Copy shared source code
COPY ["_Shared/", "_Shared/"]

# Copy service source code
COPY ["ReportsService/", "ReportsService/"]

WORKDIR "/src/ReportsService/ReportsService.Api"
RUN dotnet build "ReportsService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportsService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:80

COPY --from=publish /app/publish .

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "ReportsService.Api.dll"]
