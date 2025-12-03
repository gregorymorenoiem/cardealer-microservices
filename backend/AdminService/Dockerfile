# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["AdminService/AdminService.Api/AdminService.Api.csproj", "AdminService/AdminService.Api/"]
COPY ["AdminService/AdminService.Application/AdminService.Application.csproj", "AdminService/AdminService.Application/"]
COPY ["AdminService/AdminService.Domain/AdminService.Domain.csproj", "AdminService/AdminService.Domain/"]
COPY ["AdminService/AdminService.Infrastructure/AdminService.Infrastructure.csproj", "AdminService/AdminService.Infrastructure/"]

RUN dotnet restore "AdminService/AdminService.Api/AdminService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AdminService/AdminService.Api"
RUN dotnet build "AdminService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AdminService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "AdminService.Api.dll"]
