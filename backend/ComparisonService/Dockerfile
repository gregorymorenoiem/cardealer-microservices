# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["ComparisonService.Api/ComparisonService.Api.csproj", "ComparisonService.Api/"]
COPY ["ComparisonService.Domain/ComparisonService.Domain.csproj", "ComparisonService.Domain/"]
COPY ["ComparisonService.Infrastructure/ComparisonService.Infrastructure.csproj", "ComparisonService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ComparisonService.Api/ComparisonService.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/ComparisonService.Api"
RUN dotnet build "ComparisonService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ComparisonService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

RUN addgroup -g 1000 appuser && adduser -D -u 1000 -G appuser appuser
USER appuser
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ComparisonService.Api.dll"]
