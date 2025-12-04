# TracingService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Security hardening
RUN adduser -D -s /sbin/nologin appuser
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["TracingService.Api/TracingService.Api.csproj", "TracingService.Api/"]
COPY ["TracingService.Application/TracingService.Application.csproj", "TracingService.Application/"]
COPY ["TracingService.Domain/TracingService.Domain.csproj", "TracingService.Domain/"]
COPY ["TracingService.Infrastructure/TracingService.Infrastructure.csproj", "TracingService.Infrastructure/"]

RUN dotnet restore "TracingService.Api/TracingService.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/TracingService.Api"
RUN dotnet build "TracingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TracingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "TracingService.Api.dll"]
