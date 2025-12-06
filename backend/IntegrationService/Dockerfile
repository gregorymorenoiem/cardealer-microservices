FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IntegrationService/IntegrationService.Api/IntegrationService.Api.csproj", "IntegrationService/IntegrationService.Api/"]
COPY ["IntegrationService/IntegrationService.Application/IntegrationService.Application.csproj", "IntegrationService/IntegrationService.Application/"]
COPY ["IntegrationService/IntegrationService.Domain/IntegrationService.Domain.csproj", "IntegrationService/IntegrationService.Domain/"]
COPY ["IntegrationService/IntegrationService.Infrastructure/IntegrationService.Infrastructure.csproj", "IntegrationService/IntegrationService.Infrastructure/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
RUN dotnet restore "IntegrationService/IntegrationService.Api/IntegrationService.Api.csproj"
COPY . .
WORKDIR "/src/IntegrationService/IntegrationService.Api"
RUN dotnet build "IntegrationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IntegrationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntegrationService.Api.dll"]
