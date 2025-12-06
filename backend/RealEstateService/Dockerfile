# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["RealEstateService.Api/RealEstateService.Api.csproj", "RealEstateService.Api/"]
COPY ["RealEstateService.Application/RealEstateService.Application.csproj", "RealEstateService.Application/"]
COPY ["RealEstateService.Domain/RealEstateService.Domain.csproj", "RealEstateService.Domain/"]
COPY ["RealEstateService.Infrastructure/RealEstateService.Infrastructure.csproj", "RealEstateService.Infrastructure/"]
COPY ["../_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "../_Shared/CarDealer.Shared/"]

# Restore
RUN dotnet restore "RealEstateService.Api/RealEstateService.Api.csproj"

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

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "RealEstateService.Api.dll"]
