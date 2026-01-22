FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RegulatoryAlertService.Api/RegulatoryAlertService.Api.csproj", "RegulatoryAlertService.Api/"]
COPY ["RegulatoryAlertService.Application/RegulatoryAlertService.Application.csproj", "RegulatoryAlertService.Application/"]
COPY ["RegulatoryAlertService.Domain/RegulatoryAlertService.Domain.csproj", "RegulatoryAlertService.Domain/"]
COPY ["RegulatoryAlertService.Infrastructure/RegulatoryAlertService.Infrastructure.csproj", "RegulatoryAlertService.Infrastructure/"]
RUN dotnet restore "RegulatoryAlertService.Api/RegulatoryAlertService.Api.csproj"
COPY . .
WORKDIR "/src/RegulatoryAlertService.Api"
RUN dotnet build "RegulatoryAlertService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RegulatoryAlertService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RegulatoryAlertService.Api.dll"]
