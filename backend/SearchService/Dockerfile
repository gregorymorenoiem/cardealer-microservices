# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects first
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

# Copy project files
COPY ["SearchService/SearchService.Api/SearchService.Api.csproj", "SearchService/SearchService.Api/"]
COPY ["SearchService/SearchService.Application/SearchService.Application.csproj", "SearchService/SearchService.Application/"]
COPY ["SearchService/SearchService.Domain/SearchService.Domain.csproj", "SearchService/SearchService.Domain/"]
COPY ["SearchService/SearchService.Infrastructure/SearchService.Infrastructure.csproj", "SearchService/SearchService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SearchService/SearchService.Api/SearchService.Api.csproj"

# Copy all source code
COPY _Shared/ _Shared/
COPY SearchService/ SearchService/

# Build and publish
WORKDIR "/src/SearchService/SearchService.Api"
RUN dotnet build "SearchService.Api.csproj" -c Release -o /app/build
RUN dotnet publish "SearchService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install wget for healthcheck
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Create secrets directory
RUN mkdir -p /run/secrets

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Docker

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 80

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "SearchService.Api.dll"]
