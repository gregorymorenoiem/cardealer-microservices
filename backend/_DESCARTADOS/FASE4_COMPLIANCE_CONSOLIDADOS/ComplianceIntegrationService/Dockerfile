# =====================================================
# C12: ComplianceIntegrationService - Dockerfile
# Integración con Entes Reguladores de República Dominicana
# =====================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto
COPY ["ComplianceIntegrationService.Api/ComplianceIntegrationService.Api.csproj", "ComplianceIntegrationService.Api/"]
COPY ["ComplianceIntegrationService.Application/ComplianceIntegrationService.Application.csproj", "ComplianceIntegrationService.Application/"]
COPY ["ComplianceIntegrationService.Domain/ComplianceIntegrationService.Domain.csproj", "ComplianceIntegrationService.Domain/"]
COPY ["ComplianceIntegrationService.Infrastructure/ComplianceIntegrationService.Infrastructure.csproj", "ComplianceIntegrationService.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "ComplianceIntegrationService.Api/ComplianceIntegrationService.Api.csproj"

# Copiar código fuente
COPY . .

# Compilar
WORKDIR "/src/ComplianceIntegrationService.Api"
RUN dotnet build "ComplianceIntegrationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ComplianceIntegrationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Metadata
LABEL maintainer="OKLA Compliance Team <compliance@okla.com.do>"
LABEL description="Servicio de Integración con Entes Reguladores de República Dominicana"
LABEL version="1.0.0"

ENTRYPOINT ["dotnet", "ComplianceIntegrationService.Api.dll"]
