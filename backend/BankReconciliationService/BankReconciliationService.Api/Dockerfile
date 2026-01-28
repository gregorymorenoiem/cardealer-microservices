# See https://aka.ms/customizecontainer to learn how to customize your debug container
# and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["BankReconciliationService.Api/BankReconciliationService.Api.csproj", "BankReconciliationService.Api/"]
COPY ["BankReconciliationService.Application/BankReconciliationService.Application.csproj", "BankReconciliationService.Application/"]
COPY ["BankReconciliationService.Domain/BankReconciliationService.Domain.csproj", "BankReconciliationService.Domain/"]
COPY ["BankReconciliationService.Infrastructure/BankReconciliationService.Infrastructure.csproj", "BankReconciliationService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "BankReconciliationService.Api/BankReconciliationService.Api.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/BankReconciliationService.Api"
RUN dotnet build "BankReconciliationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BankReconciliationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankReconciliationService.Api.dll"]
