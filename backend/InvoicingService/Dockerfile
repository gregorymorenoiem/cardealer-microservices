# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy csproj files and restore
COPY ["InvoicingService/InvoicingService.Api/InvoicingService.Api.csproj", "InvoicingService/InvoicingService.Api/"]
COPY ["InvoicingService/InvoicingService.Application/InvoicingService.Application.csproj", "InvoicingService/InvoicingService.Application/"]
COPY ["InvoicingService/InvoicingService.Domain/InvoicingService.Domain.csproj", "InvoicingService/InvoicingService.Domain/"]
COPY ["InvoicingService/InvoicingService.Infrastructure/InvoicingService.Infrastructure.csproj", "InvoicingService/InvoicingService.Infrastructure/"]

RUN dotnet restore "InvoicingService/InvoicingService.Api/InvoicingService.Api.csproj"

# Copy everything else and build
COPY ["_Shared/", "_Shared/"]
COPY ["InvoicingService/", "InvoicingService/"]
WORKDIR "/src/InvoicingService/InvoicingService.Api"
RUN dotnet build "InvoicingService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "InvoicingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create secrets directory for Docker secrets/Kubernetes secrets
RUN mkdir -p /run/secrets

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Set environment for Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "InvoicingService.Api.dll"]
