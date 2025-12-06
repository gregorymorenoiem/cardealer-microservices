FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinanceService/FinanceService.Api/FinanceService.Api.csproj", "FinanceService/FinanceService.Api/"]
COPY ["FinanceService/FinanceService.Application/FinanceService.Application.csproj", "FinanceService/FinanceService.Application/"]
COPY ["FinanceService/FinanceService.Domain/FinanceService.Domain.csproj", "FinanceService/FinanceService.Domain/"]
COPY ["FinanceService/FinanceService.Infrastructure/FinanceService.Infrastructure.csproj", "FinanceService/FinanceService.Infrastructure/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]
RUN dotnet restore "FinanceService/FinanceService.Api/FinanceService.Api.csproj"
COPY . .
WORKDIR "/src/FinanceService/FinanceService.Api"
RUN dotnet build "FinanceService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceService.Api.dll"]
