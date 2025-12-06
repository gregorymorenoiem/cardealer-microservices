FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MarketingService/MarketingService.Api/MarketingService.Api.csproj", "MarketingService/MarketingService.Api/"]
COPY ["MarketingService/MarketingService.Application/MarketingService.Application.csproj", "MarketingService/MarketingService.Application/"]
COPY ["MarketingService/MarketingService.Domain/MarketingService.Domain.csproj", "MarketingService/MarketingService.Domain/"]
COPY ["MarketingService/MarketingService.Infrastructure/MarketingService.Infrastructure.csproj", "MarketingService/MarketingService.Infrastructure/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
RUN dotnet restore "MarketingService/MarketingService.Api/MarketingService.Api.csproj"
COPY . .
WORKDIR "/src/MarketingService/MarketingService.Api"
RUN dotnet build "MarketingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarketingService.Api.dll"]
