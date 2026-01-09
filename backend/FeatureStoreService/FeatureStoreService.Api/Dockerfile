FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FeatureStoreService.Api/FeatureStoreService.Api.csproj", "FeatureStoreService.Api/"]
COPY ["FeatureStoreService.Application/FeatureStoreService.Application.csproj", "FeatureStoreService.Application/"]
COPY ["FeatureStoreService.Domain/FeatureStoreService.Domain.csproj", "FeatureStoreService.Domain/"]
COPY ["FeatureStoreService.Infrastructure/FeatureStoreService.Infrastructure.csproj", "FeatureStoreService.Infrastructure/"]
RUN dotnet restore "FeatureStoreService.Api/FeatureStoreService.Api.csproj"
COPY . .
WORKDIR "/src/FeatureStoreService.Api"
RUN dotnet build "FeatureStoreService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FeatureStoreService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FeatureStoreService.Api.dll"]
