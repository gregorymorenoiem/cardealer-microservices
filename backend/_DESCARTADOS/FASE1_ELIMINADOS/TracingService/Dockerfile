# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy solution and project files
COPY ["TracingService/TracingService.Api/TracingService.Api.csproj", "TracingService/TracingService.Api/"]
COPY ["TracingService/TracingService.Application/TracingService.Application.csproj", "TracingService/TracingService.Application/"]
COPY ["TracingService/TracingService.Domain/TracingService.Domain.csproj", "TracingService/TracingService.Domain/"]
COPY ["TracingService/TracingService.Infrastructure/TracingService.Infrastructure.csproj", "TracingService/TracingService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TracingService/TracingService.Api/TracingService.Api.csproj"

# Copy the rest of the code
COPY _Shared/ _Shared/
COPY TracingService/ TracingService/

# Build
WORKDIR "/src/TracingService/TracingService.Api"
RUN dotnet build "TracingService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TracingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create secrets directory for Docker secrets
RUN mkdir -p /run/secrets

# Install wget for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends wget && rm -rf /var/lib/apt/lists/*

# Set environment to Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TracingService.Api.dll"]
