FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["VehicleIntelligenceService/VehicleIntelligenceService.Api/VehicleIntelligenceService.Api.csproj", "VehicleIntelligenceService/VehicleIntelligenceService.Api/"]
COPY ["VehicleIntelligenceService/VehicleIntelligenceService.Application/VehicleIntelligenceService.Application.csproj", "VehicleIntelligenceService/VehicleIntelligenceService.Application/"]
COPY ["VehicleIntelligenceService/VehicleIntelligenceService.Domain/VehicleIntelligenceService.Domain.csproj", "VehicleIntelligenceService/VehicleIntelligenceService.Domain/"]
COPY ["VehicleIntelligenceService/VehicleIntelligenceService.Infrastructure/VehicleIntelligenceService.Infrastructure.csproj", "VehicleIntelligenceService/VehicleIntelligenceService.Infrastructure/"]

RUN dotnet restore "VehicleIntelligenceService/VehicleIntelligenceService.Api/VehicleIntelligenceService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/VehicleIntelligenceService/VehicleIntelligenceService.Api"
RUN dotnet build "VehicleIntelligenceService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VehicleIntelligenceService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VehicleIntelligenceService.Api.dll"]
