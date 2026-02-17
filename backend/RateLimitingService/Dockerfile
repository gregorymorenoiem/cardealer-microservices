FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /src

# Copy everything and restore
COPY . .
RUN dotnet restore "RateLimitingService/RateLimitingService.Api/RateLimitingService.Api.csproj"

# Build
RUN dotnet build "RateLimitingService/RateLimitingService.Api/RateLimitingService.Api.csproj" -c Release -o /app/build --no-restore

# Change to build output directory
WORKDIR /app/build

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

EXPOSE 8080

ENTRYPOINT ["dotnet", "RateLimitingService.Api.dll"]
