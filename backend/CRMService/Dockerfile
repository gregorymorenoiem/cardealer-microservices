FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create secrets directory for Docker secrets
RUN mkdir -p /run/secrets

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy project files
COPY ["CRMService/CRMService.Api/CRMService.Api.csproj", "CRMService/CRMService.Api/"]
COPY ["CRMService/CRMService.Application/CRMService.Application.csproj", "CRMService/CRMService.Application/"]
COPY ["CRMService/CRMService.Domain/CRMService.Domain.csproj", "CRMService/CRMService.Domain/"]
COPY ["CRMService/CRMService.Infrastructure/CRMService.Infrastructure.csproj", "CRMService/CRMService.Infrastructure/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]

# Restore dependencies
RUN dotnet restore "CRMService/CRMService.Api/CRMService.Api.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/CRMService/CRMService.Api"
RUN dotnet build "CRMService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CRMService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Set environment to Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "CRMService.Api.dll"]
