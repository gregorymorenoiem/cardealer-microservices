# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["AlertService.Api/AlertService.Api.csproj", "AlertService.Api/"]
COPY ["AlertService.Domain/AlertService.Domain.csproj", "AlertService.Domain/"]
COPY ["AlertService.Infrastructure/AlertService.Infrastructure.csproj", "AlertService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "AlertService.Api/AlertService.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/AlertService.Api"
RUN dotnet build "AlertService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "AlertService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "AlertService.Api.dll"]
