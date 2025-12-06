FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ReportsService/ReportsService.Api/ReportsService.Api.csproj", "ReportsService/ReportsService.Api/"]
COPY ["ReportsService/ReportsService.Application/ReportsService.Application.csproj", "ReportsService/ReportsService.Application/"]
COPY ["ReportsService/ReportsService.Domain/ReportsService.Domain.csproj", "ReportsService/ReportsService.Domain/"]
COPY ["ReportsService/ReportsService.Infrastructure/ReportsService.Infrastructure.csproj", "ReportsService/ReportsService.Infrastructure/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
RUN dotnet restore "ReportsService/ReportsService.Api/ReportsService.Api.csproj"
COPY . .
WORKDIR "/src/ReportsService/ReportsService.Api"
RUN dotnet build "ReportsService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReportsService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReportsService.Api.dll"]
