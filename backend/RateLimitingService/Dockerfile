# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["RateLimitingService.Api/RateLimitingService.Api.csproj", "RateLimitingService.Api/"]
COPY ["RateLimitingService.Core/RateLimitingService.Core.csproj", "RateLimitingService.Core/"]
RUN dotnet restore "RateLimitingService.Api/RateLimitingService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/RateLimitingService.Api"
RUN dotnet build "RateLimitingService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RateLimitingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "RateLimitingService.Api.dll"]
