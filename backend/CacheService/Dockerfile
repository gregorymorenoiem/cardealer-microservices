FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5095
EXPOSE 7095

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["CacheService.Api/CacheService.Api.csproj", "CacheService.Api/"]
COPY ["CacheService.Application/CacheService.Application.csproj", "CacheService.Application/"]
COPY ["CacheService.Domain/CacheService.Domain.csproj", "CacheService.Domain/"]
COPY ["CacheService.Infrastructure/CacheService.Infrastructure.csproj", "CacheService.Infrastructure/"]

RUN dotnet restore "CacheService.Api/CacheService.Api.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/CacheService.Api"
RUN dotnet build "CacheService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CacheService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CacheService.Api.dll"]
