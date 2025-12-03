FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FeatureToggleService/FeatureToggleService.Api/FeatureToggleService.Api.csproj", "FeatureToggleService/FeatureToggleService.Api/"]
COPY ["FeatureToggleService/FeatureToggleService.Application/FeatureToggleService.Application.csproj", "FeatureToggleService/FeatureToggleService.Application/"]
COPY ["FeatureToggleService/FeatureToggleService.Domain/FeatureToggleService.Domain.csproj", "FeatureToggleService/FeatureToggleService.Domain/"]
COPY ["FeatureToggleService/FeatureToggleService.Infrastructure/FeatureToggleService.Infrastructure.csproj", "FeatureToggleService/FeatureToggleService.Infrastructure/"]
RUN dotnet restore "FeatureToggleService/FeatureToggleService.Api/FeatureToggleService.Api.csproj"
COPY . .
WORKDIR "/src/FeatureToggleService/FeatureToggleService.Api"
RUN dotnet build "FeatureToggleService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FeatureToggleService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FeatureToggleService.Api.dll"]
