# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["InventoryManagementService/InventoryManagementService.Api/InventoryManagementService.Api.csproj", "InventoryManagementService.Api/"]
COPY ["InventoryManagementService/InventoryManagementService.Application/InventoryManagementService.Application.csproj", "InventoryManagementService.Application/"]
COPY ["InventoryManagementService/InventoryManagementService.Infrastructure/InventoryManagementService.Infrastructure.csproj", "InventoryManagementService.Infrastructure/"]
COPY ["InventoryManagementService/InventoryManagementService.Domain/InventoryManagementService.Domain.csproj", "InventoryManagementService.Domain/"]

RUN dotnet restore "InventoryManagementService.Api/InventoryManagementService.Api.csproj"

# Copy everything else and build
COPY InventoryManagementService/ .
WORKDIR "/src/InventoryManagementService.Api"
RUN dotnet build "InventoryManagementService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "InventoryManagementService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
RUN apk add --no-cache wget
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
RUN addgroup -g 1000 appuser && adduser -D -u 1000 -G appuser appuser
USER appuser
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "InventoryManagementService.Api.dll"]
