# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["VehicleService/VehicleService.Api/VehicleService.Api.csproj", "VehicleService/VehicleService.Api/"]
COPY ["VehicleService/VehicleService.Application/VehicleService.Application.csproj", "VehicleService/VehicleService.Application/"]
COPY ["VehicleService/VehicleService.Domain/VehicleService.Domain.csproj", "VehicleService/VehicleService.Domain/"]
COPY ["VehicleService/VehicleService.Infrastructure/VehicleService.Infrastructure.csproj", "VehicleService/VehicleService.Infrastructure/"]

RUN dotnet restore "VehicleService/VehicleService.Api/VehicleService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/VehicleService/VehicleService.Api"
RUN dotnet build "VehicleService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "VehicleService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "VehicleService.Api.dll"]
