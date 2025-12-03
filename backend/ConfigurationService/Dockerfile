# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["ConfigurationService/ConfigurationService.Api/ConfigurationService.Api.csproj", "ConfigurationService/ConfigurationService.Api/"]
COPY ["ConfigurationService/ConfigurationService.Application/ConfigurationService.Application.csproj", "ConfigurationService/ConfigurationService.Application/"]
COPY ["ConfigurationService/ConfigurationService.Domain/ConfigurationService.Domain.csproj", "ConfigurationService/ConfigurationService.Domain/"]
COPY ["ConfigurationService/ConfigurationService.Infrastructure/ConfigurationService.Infrastructure.csproj", "ConfigurationService/ConfigurationService.Infrastructure/"]

RUN dotnet restore "ConfigurationService/ConfigurationService.Api/ConfigurationService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ConfigurationService/ConfigurationService.Api"
RUN dotnet build "ConfigurationService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ConfigurationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - Alpine para menor superficie de ataque
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user (Alpine uses addgroup/adduser)
RUN addgroup -g 1000 appuser && \
    adduser -D -u 1000 -G appuser appuser && \
    chown -R appuser:appuser /app

USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD dotnet /app/ConfigurationService.Api.dll --help > /dev/null 2>&1 || exit 1

ENTRYPOINT ["dotnet", "ConfigurationService.Api.dll"]