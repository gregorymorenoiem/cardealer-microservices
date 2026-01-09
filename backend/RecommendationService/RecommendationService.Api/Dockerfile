FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RecommendationService/RecommendationService.Api/RecommendationService.Api.csproj", "RecommendationService/RecommendationService.Api/"]
COPY ["RecommendationService/RecommendationService.Application/RecommendationService.Application.csproj", "RecommendationService/RecommendationService.Application/"]
COPY ["RecommendationService/RecommendationService.Domain/RecommendationService.Domain.csproj", "RecommendationService/RecommendationService.Domain/"]
COPY ["RecommendationService/RecommendationService.Infrastructure/RecommendationService.Infrastructure.csproj", "RecommendationService/RecommendationService.Infrastructure/"]

RUN dotnet restore "RecommendationService/RecommendationService.Api/RecommendationService.Api.csproj"

COPY . .
WORKDIR "/src/RecommendationService/RecommendationService.Api"
RUN dotnet build "RecommendationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecommendationService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecommendationService.Api.dll"]
