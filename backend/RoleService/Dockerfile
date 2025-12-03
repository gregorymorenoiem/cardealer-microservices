# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["RoleService/RoleService.Api/RoleService.Api.csproj", "RoleService/RoleService.Api/"]
COPY ["RoleService/RoleService.Application/RoleService.Application.csproj", "RoleService/RoleService.Application/"]
COPY ["RoleService/RoleService.Domain/RoleService.Domain.csproj", "RoleService/RoleService.Domain/"]
COPY ["RoleService/RoleService.Infrastructure/RoleService.Infrastructure.csproj", "RoleService/RoleService.Infrastructure/"]

RUN dotnet restore "RoleService/RoleService.Api/RoleService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/RoleService/RoleService.Api"
RUN dotnet build "RoleService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RoleService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "RoleService.Api.dll"]
