# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["AdminService/AdminService.Api/AdminService.Api.csproj", "AdminService/AdminService.Api/"]
COPY ["AdminService/AdminService.Application/AdminService.Application.csproj", "AdminService/AdminService.Application/"]
COPY ["AdminService/AdminService.Domain/AdminService.Domain.csproj", "AdminService/AdminService.Domain/"]
COPY ["AdminService/AdminService.Infrastructure/AdminService.Infrastructure.csproj", "AdminService/AdminService.Infrastructure/"]
COPY ["AdminService/AdminService.Shared/AdminService.Shared.csproj", "AdminService/AdminService.Shared/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]

RUN dotnet restore "AdminService/AdminService.Api/AdminService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AdminService/AdminService.Api"
RUN dotnet build "AdminService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AdminService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 80

# Create directories for secrets
RUN mkdir -p /run/secrets

# Create non-root user (Alpine syntax)
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app /run/secrets

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

# Environment variables for secrets (can be overridden)
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:80

# Health check using wget (lighter than curl on Alpine)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 http://localhost:80/health -qO- > /dev/null || exit 1

ENTRYPOINT ["dotnet", "AdminService.Api.dll"]
