# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["InventoryManagementService.Api/InventoryManagementService.Api.csproj", "InventoryManagementService.Api/"]
COPY ["InventoryManagementService.Application/InventoryManagementService.Application.csproj", "InventoryManagementService.Application/"]
COPY ["InventoryManagementService.Infrastructure/InventoryManagementService.Infrastructure.csproj", "InventoryManagementService.Infrastructure/"]
COPY ["InventoryManagementService.Domain/InventoryManagementService.Domain.csproj", "InventoryManagementService.Domain/"]

RUN dotnet restore "InventoryManagementService.Api/InventoryManagementService.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/InventoryManagementService.Api"
RUN dotnet build "InventoryManagementService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "InventoryManagementService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryManagementService.Api.dll"]
