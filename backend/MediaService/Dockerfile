# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["MediaService/MediaService.Api/MediaService.Api.csproj", "MediaService/MediaService.Api/"]
COPY ["MediaService/MediaService.Application/MediaService.Application.csproj", "MediaService/MediaService.Application/"]
COPY ["MediaService/MediaService.Domain/MediaService.Domain.csproj", "MediaService/MediaService.Domain/"]
COPY ["MediaService/MediaService.Infrastructure/MediaService.Infrastructure.csproj", "MediaService/MediaService.Infrastructure/"]
COPY ["MediaService/MediaService.Shared/MediaService.Shared.csproj", "MediaService/MediaService.Shared/"]

RUN dotnet restore "MediaService/MediaService.Api/MediaService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MediaService/MediaService.Api"
RUN dotnet build "MediaService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MediaService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user and uploads directory
RUN useradd -m -u 1000 appuser && \
    mkdir -p /app/uploads && \
    chown -R appuser:appuser /app
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "MediaService.Api.dll"]